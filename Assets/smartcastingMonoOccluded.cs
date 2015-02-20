using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System;
using SocketIOClient;
using System.IO;
using UnityEngine;
using System.Collections;

public class smartcastingMonoOccluded : MonoBehaviour {
	
	Quaternion originalRotation;
	//	private string display = "Press the 1 and 2 buttons on the Wiimote to connect.";
	public List<float> selectionTimeSmall;
	public List<float> selectionTimeLarge;
	private float currentTimeAtSelectionTrigger = 0;
	private const float deadZoneBegin = 0.133F;
	private const float deadZoneEnd = 0.99F;
	private float gyroDrift = 0;	
	private bool timeOfSelectionTriggerRegistered = false;
	private int selectionErrorSmall = 0;
	private int selectionErrorLarge = 0;
	public List<string> jsonLog;
	public List<Task> taskList;
	public int currentTaskNo = 0;
	public const int noOfTasks = 32;
	public float logFrequency = 0.025f;
	public InteractorState interactorState = new InteractorState();
	public TargetState targetState = new TargetState();
	public InteractionPlaneState interactionPlaneState = new InteractionPlaneState();
	private const string ARRAY_FLAG = "[";
	private float oldTargetX = 0.0f;
	private float gyroX = 0.0f;
	private float gyroY = 0.0f;
	private float gyroZ = 0.0f;
	private float oldGyroX = 0.0f;
	private float oldGyroY = 0.0f;
	private float oldGyroZ = 0.0f;
	private float tapX = 0.0f;
	private float tapY = 0.76f;
	private float oldTapX = 0.0f;
	private float oldTapY = 0.0f;
	private float old_ir_x = 0.0f;
	private float old_ir_y = 0.0f;
	private float old_wiimote_pitch = 0.0f;
	private float startSelectedTime = 0.0f;
	private float targetSelectedTime = 0.0f;
	private float targetEnteredTime = 0.0f;
	private float targetZAngleOffset = 0f;
	private Client client = new Client ("http://198.61.165.79:3000"); //new Client ("http://192.168.173.1:3000");//new Client("http://198.61.165.79:8091");//new Client("http://24.246.24.104:8091"); 
	private bool dragging = false;
	private bool selectionTriggered = false;
	private bool dockingTriggered = false;
	private bool oldSelectionTriggered = false;
	private Color originalColor;
	private static GUIText text;
	public bool startSelected = false;
	private string sufix = "0000";
	public int score = 0;

	private static IntPtr lib;
	
	public class ObjectLog {
		public float x = 0.0f;
		public float y = 0.0f;
		public float z = 0.0f;
		public float rx = 0.0f;
		public float ry = 0.0f;
		public float rz = 0.0f;
		public float timestamp;
		public int taskId = -1;
		public string name;
		public Color color;
		
	}
	
	public class TargetState: ObjectLog {
	}
	
	public class InteractorState: ObjectLog {
		public float localX = 0.0f;
		public float localY = 0.0f;
		public float localZ = 0.0f;
	}
	
	public class InteractionPlaneState: ObjectLog {
	}
	
	public abstract class Task {
		public int no;
		public string name;
		public string instructions;
		public bool completed;
		
		public abstract bool isCompleted ();
		
		public abstract void start();
		
	}
	
	public class TargetingTask : Task {
		
		public Vector3 targetInit;
		public float targetScale, distributionRadius, timeout, startTime, timeLeft;
		private GameObject target,startT;
		private List<GameObject> occluders;
		
		public TargetingTask(int taskNo, float distributionR, float targetR, int timeout) {
			no = taskNo;
			target = GameObject.Find("target");
			startT = GameObject.Find("start");
			targetScale = targetR;
			distributionRadius = distributionR;
			this.timeout = timeout;
			occluders = new List<GameObject> ();
			for (int i = 1; i <=43; i++)
				occluders.Add (GameObject.Find("occluder"+i.ToString()));
			name = "Select yellow sphere, then select the red spehere as quickly and precisely as you can";
			targetInit = randomPointOnSphere (startT.transform.position, 4f,0f,1f,0.7f,1.0f);
		}
		
		public override void start(){
			//target.transform.position = targetInit;
			//target.transform.localScale = new Vector3 (targetScale, targetScale, targetScale); 			
			//target.renderer.material.color = Color.red;
			startT.renderer.material.color = Color.yellow;
			startT.transform.position = new Vector3(1000f,4.5f,0.05f);
			
			occluders [0].transform.position = targetInit - new Vector3 (1110f, 0f, 2.0f);
			occluders [1].transform.position = targetInit - new Vector3 (1110f, 1.7f, 0f);
			occluders [2].transform.position = targetInit - new Vector3 (1111.7f, 0f, 0f);
			occluders [3].transform.position = targetInit + new Vector3 (1110f, 0f, 1.7f);
			occluders [4].transform.position = targetInit + new Vector3 (1110f, 1.7f, 0f);
			occluders [5].transform.position = targetInit + new Vector3 (1111.7f, 0f, 0f);
			
			for (int i = 6; i <=42; i++) {
				float randx = UnityEngine.Random.Range (-9f, 9f)+ 1111.7f;
				float randy = UnityEngine.Random.Range (0.2f, 9f);
				float randz = UnityEngine.Random.Range (1f, 14f);
				
				// make sure that occluders don't overlap the target
				while ( Mathf.Abs(randx-targetInit.x) < targetScale +0.25 && Mathf.Abs(randy-targetInit.y) < targetScale +0.25 && Mathf.Abs(randz-targetInit.z) < targetScale +0.25) {
					randx = UnityEngine.Random.Range (-9f, 9f) + 1111.7f;
					randy = UnityEngine.Random.Range (0.2f, 9f);
					randz = UnityEngine.Random.Range (1f, 14f);
				}
				occluders [i].transform.position = new Vector3 (randx, randy, randz);
			}
			
			occluders[0].renderer.material.color = Color.blue;
			for (int i = 1; i <=42; i++) { 
				occluders[i].renderer.material.color = Color.blue;
				float scale = UnityEngine.Random.Range(0.75f,1.5f);
				occluders[i].transform.localScale = new Vector3 (scale,scale, scale);
			}
			startTime = Time.realtimeSinceStartup;
		}
		
		public override bool isCompleted(){
			timeLeft = Mathf.Floor(timeout - (Time.realtimeSinceStartup - startTime));
			if (timeLeft <= 0) {
				completed = true;
			}
			if (target.renderer.material.color == Color.green) { 
				completed = true;
			}
			return completed;
		}
		
		private Vector3 randomPointOnSphere(Vector3 centre, float radius, float ul, float uh, float vl, float vh) {
			
			float u = UnityEngine.Random.Range (ul, uh);
			float v = UnityEngine.Random.Range (vh, vl);
			float theta = Mathf.PI * 2f * u;
			float phi = Mathf.Acos (2f * v - 1f);
			float y = centre.y + (radius * (float) Mathf.Sin (phi) * (float) Math.Cos (theta));
			float x = centre.x + (radius * (float) Mathf.Sin (phi) * (float) Math.Sin (theta));
			float z = centre.z + (radius * (float) Mathf.Cos (phi));
			return new Vector3 (x, y, z);
			
		}
	}
	
	private void OnApplicationQuit() {
		client.Close();
		client.Dispose();
	}
	
	//connection opened event.
	private void SocketOpened (object sender, EventArgs e){
		print("The socketIO opened!");
	}
	
	private void SocketMessage (object sender, MessageEventArgs e) {
		
		if ( e!= null ) {
			string msg = e.Message.MessageText;
			if (msg != null && msg.IndexOf(ARRAY_FLAG) == 0) {
				this.processMessageBatch(msg);
			} else {
				this.processMessage(msg);
			}
		} 
	}
	
	//Conne  socket.emit('news', { hello: 'world' });
	private void SocketConnectionClosed (object sender, EventArgs e) {
		print("WebSocketConnection was terminated!");
	}
	
	//Connection error event.
	private void SocketError (object sender, SocketIOClient.ErrorEventArgs e) {
		print("socket client error:");
		print(e.Message);
	}
	
	private void processMessage(string msg) {
		
		
		JsonObject obj = (JsonObject) SimpleJson.SimpleJson.DeserializeObject(msg);
		//print(obj);
		
		object name = null;
		object args = null;
		object alpha = null;
		object beta = null;
		object gamma = null;
		object mx = null;
		object my = null;
		
		if (obj != null) {
			if (obj.TryGetValue ("name", out name)) {
				if (name != null) {
					if (name.ToString().Equals("gyro")) {
						
						obj.TryGetValue ("args", out args);
						
						JsonArray jsonArray = (JsonArray) SimpleJson.SimpleJson.DeserializeObject(args.ToString());
						JsonObject gyroValues = (JsonObject) SimpleJson.SimpleJson.DeserializeObject(jsonArray[0].ToString());
						
						/*gyroValues.TryGetValue("gamma", out gamma);
						float gyroZtemp = float.Parse (gamma.ToString());
						gyroZ = gyroZtemp;//(gyroZtemp < -89f) ? gyroZ = -89f : ((gyroZtemp > -1f) ? gyroZ = -1f : gyroZ = gyroZtemp);*/
						
						gyroValues.TryGetValue("alpha", out alpha);
						float gyroYtemp = float.Parse (alpha.ToString());
						gyroY = -gyroYtemp; //(gyroYtemp < f) ? gyroY = 89f : ((gyroYtemp > -1f) ? gyroY = -1f : gyroY = gyroYtemp);
						
						gyroValues.TryGetValue("beta", out alpha);
						float gyroXtemp = float.Parse (alpha.ToString());
						gyroX = -gyroXtemp;//(gyroXtemp < -89f) ? gyroX = -89f : ((gyroXtemp > -1f) ? gyroX = -1f : gyroX = gyroXtemp);

						gyroValues.TryGetValue("gamma", out alpha);
						float gyroZtemp = float.Parse (alpha.ToString());
						gyroZ = -gyroZtemp;//(gyroXtemp < -89f) ? gyroX = -89f : ((gyroXtemp > -1f) ? gyroX = -1f : gyroX = gyroXtemp);
						

					}
					if (name.ToString().Equals("tapstart")) {
						//selectionTriggered = false;
						dockingTriggered = true;
					}
					if (name.ToString().Equals("tapmove")) {
						obj.TryGetValue ("args", out args);
						
						JsonArray jsonArray = (JsonArray) SimpleJson.SimpleJson.DeserializeObject(args.ToString());
						JsonObject touchStartValues = (JsonObject) SimpleJson.SimpleJson.DeserializeObject(jsonArray[0].ToString());
						
						touchStartValues.TryGetValue("mx", out mx);
						//tapX =  (float.Parse (mx.ToString());
						
						touchStartValues.TryGetValue("my", out my);
						float tempTapY = float.Parse (my.ToString());
						
						if ( tempTapY > oldTapY) {
							if (tapY >= 0.133f)
								tapY = tapY - 0.033f;//(oldTapY > float.Parse (mx.ToString())) ? (tapY + 1f) : (tapY - 1f);//1.0f - float.Parse (my.ToString())+0.02f;
						} else if (tempTapY < oldTapY) {
							if (tapY <= 2.99f)
								tapY = tapY + 0.033f;
						}
						oldTapY = tempTapY;
						dragging = true;
						//Debug.Log (tapX.ToString()+","+tapY.ToString());
					}
					
					
					if (name.ToString().Equals("tapend")) {
						//dragging = false;
						//tapX = 100.0f;
						//tapY = 100.0f;
						selectionTriggered = true;
						//dockingTriggered = false;
						//timeOfSelectionTriggerRegistered = false;
					}
					
					/*if (name.ToString().Equals("select")) {

						Debug.Log ("SELECT!!!");
					}*/
					
				}
			}
		}
		
	}
	
	//Processes the message and invoke callback or event.
	private void processMessageBatch(string msgs){
		JsonArray jsonArray = (JsonArray) SimpleJson.SimpleJson.DeserializeObject(msgs); 
		int length = jsonArray.Count;
		for (int i = 0; i < length; i++) {
			this.processMessage(jsonArray[i].ToString());
		}
	}
	
	// Use this for initialization
	void Start () {
		
		text =  (GUIText) GameObject.Find ("instructions").guiText;
		client.Opened += SocketOpened;
		client.Message += SocketMessage;
		client.SocketConnectionClosed += SocketConnectionClosed;
		client.Error +=SocketError;
		client.Connect();
		
		jsonLog = new List<string> ();
		taskList = new List<Task> ();
		sufix = UnityEngine.Random.Range(10000,99999).ToString();
		jsonLog.Add ("SmartcastingMonoOccluded,timestamp,object, pitch, yaw, cursor x,cursor y,cursor z, cursor local x, cursor local y, target x, target y, target z, target scale, action");
		// add tasks
		createTasks();
		
		// start with first task
		taskList[currentTaskNo].start();
		
		// log everything
		InvokeRepeating ("LogObjects", 0, logFrequency); 
		Application.targetFrameRate = 60;
		
	}
	
	void LogObjects() {
		
		float currentTime = Time.realtimeSinceStartup;
		text.text = "";// "Selection Task " + (currentTaskNo + 1).ToString () + " of " + noOfTasks.ToString() + "   " +((TargetingTask)taskList [currentTaskNo]).timeLeft.ToString () + " sec";		

		GameObject interactionPlane = GameObject.Find("interactionPlane");	
		
		if (dragging && (transform.position.x != interactorState.x || transform.position.y != interactorState.y || transform.position.z != interactorState.z)) {
			interactorState.z = transform.position.z;
			interactorState.y = transform.position.y;
			interactorState.x = transform.position.x;
			jsonLog.Add("SmartcastingMonoOccluded,"+taskList[currentTaskNo].no.ToString() + "," + currentTime + ",interactor," + (360f-transform.parent.transform.rotation.eulerAngles.x).ToString() + "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + ","  + transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString() + "," + transform.localPosition.x.ToString()  + "," + transform.localPosition.y.ToString() + " , , , , ,cursor moves");
		}
		
		if (interactionPlane.transform.rotation.x != interactionPlaneState.rx) {
			interactionPlaneState.rx = interactionPlane.transform.rotation.x;
			jsonLog.Add("SmartcastingMonoOccluded,"+taskList[currentTaskNo].no.ToString() + "," + currentTime + ",interactor," + (360f-transform.parent.transform.rotation.eulerAngles.x).ToString() + "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + ","  + transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString() + "," + transform.localPosition.x.ToString()  + "," + transform.localPosition.y.ToString() + " , , , , ,plane tilted");
		}
	}
	
	void OnCollisionEnter(Collision c)
	{
		//if selected, change target and interactor color
		if (c.gameObject.name == "target" && startSelected) {
			targetEnteredTime = Time.realtimeSinceStartup-startSelectedTime;
			jsonLog.Add("SmartcastingMonoOccluded," + taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",interactor," + (360f-transform.parent.transform.rotation.eulerAngles.x).ToString() + "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + "," + transform.position.x.ToString() + "," 
			            + transform.position.y.ToString() + "," + transform.position.z.ToString()  + "," + transform.localPosition.x.ToString() + "," + transform.localPosition.y.ToString() + "," 
			            + c.gameObject.transform.position.x.ToString() + "," + c.gameObject.transform.position.y.ToString() + "," + c.gameObject.transform.position.z.ToString() +", "+ c.gameObject.transform.localScale.x.ToString()+ ",target entered,"+targetEnteredTime.ToString());
			c.gameObject.renderer.material.color = Color.magenta;
			Debug.Log("Target entered");
		}
		
		if (c.gameObject.name == "start") {
			jsonLog.Add ("SmartcastingMonoOccluded," +taskList [currentTaskNo].no.ToString () + "," + Time.realtimeSinceStartup.ToString () + ",interactor," + (360f - transform.parent.transform.rotation.eulerAngles.x).ToString () +  "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + ","+ transform.position.x.ToString () + "," + transform.position.y.ToString () + "," + transform.position.z.ToString () + "," + transform.position.z.ToString () + "," + transform.localPosition.x.ToString () + " , , , , ,start entered");
			c.gameObject.renderer.material.color = Color.magenta;
			Debug.Log("Start entered");
		}
		if (c.gameObject.name.StartsWith ("occluder")) {
			c.gameObject.renderer.material.color = Color.magenta;
			Debug.Log("Occluder entered");
		}
		//if selected, change target and interactor color
		if (c.gameObject.name == "target" & dockingTriggered) {
			c.gameObject.transform.parent = transform; 
			targetZAngleOffset = c.gameObject.transform.rotation.eulerAngles.z - gyroZ*3;
			Debug.Log("In docking");
		}

	}
	
	void OnCollisionStay(Collision c)
	{
		Debug.Log("In collision");
		if (c.gameObject.name == "target" && selectionTriggered) {
			dockingTriggered = false;
			selectionTriggered = false;
			c.gameObject.renderer.material.color = Color.green;
			//c.gameObject.transform.parent = null;

			//score +=1;
			Debug.Log("Target selected");
		} 

		if (c.gameObject.name == "start" && selectionTriggered ) {
			Debug.Log("Start selected");
			startSelectedTime = Time.realtimeSinceStartup;
			startSelected = true;
			jsonLog.Add("SmartcastingMonoOccluded," +taskList[currentTaskNo].no.ToString() + "," + startSelectedTime+ ",interactor," + (360f-transform.parent.transform.rotation.eulerAngles.x).ToString() + "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + "," + transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString() + "," + transform.position.z.ToString()  + "," + transform.localPosition.x.ToString() + " , , , , ,start selected");
			c.gameObject.renderer.material.color = Color.green;
			c.gameObject.transform.position = new Vector3(100f,100f,100f);
			
		}
		
		if (c.gameObject.name.StartsWith("occluder") && selectionTriggered) {
			Debug.Log("Occluder selected");
			c.gameObject.renderer.material.color = Color.magenta;
			jsonLog.Add("SmartcastingMonoOccluded," +taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ","+c.gameObject.name+"," + (360f-transform.parent.transform.rotation.eulerAngles.x).ToString() + "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + ","+ transform.position.x.ToString() + "," 
			            + transform.position.y.ToString() + "," + transform.position.z.ToString()  + "," + transform.localPosition.x.ToString() + "," + transform.localPosition.y.ToString() + "," 
			            + c.gameObject.transform.position.x.ToString() + "," + c.gameObject.transform.position.y.ToString() + "," + c.gameObject.transform.position.z.ToString() +", "+ c.gameObject.transform.localScale.x.ToString()+ ",WRONG target selected");
			
			if (((TargetingTask)taskList[currentTaskNo]).targetScale >  0.9f) 
				selectionErrorLarge++;
			else 
				selectionErrorSmall++;
		}
	}
	
	void OnCollisionExit(Collision c)
	{
		if (c.gameObject.name == "target" && startSelected && c.gameObject.renderer.material.color != Color.green) {
			targetEnteredTime = Time.realtimeSinceStartup-startSelectedTime;
			jsonLog.Add("SmartcastingMonoOccluded," +taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",interactor," + (360f-transform.parent.transform.rotation.eulerAngles.x).ToString() + "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + ","+ transform.position.x.ToString() + "," 
			            + transform.position.y.ToString() + "," + transform.position.z.ToString()  + "," + transform.localPosition.x.ToString() + "," + transform.localPosition.y.ToString() + "," 
			            + c.gameObject.transform.position.x.ToString() + "," + c.gameObject.transform.position.y.ToString() + "," + c.gameObject.transform.position.z.ToString() +", "+ c.gameObject.transform.localScale.x.ToString()+ ",target exited,"+targetEnteredTime.ToString());
			c.gameObject.renderer.material.color = Color.red;
			Debug.Log("Target existed");
		}
		
		if (c.gameObject.name == "start") {
//			jsonLog.Add ("SmartcastingMonoOccluded," +taskList [currentTaskNo].no.ToString () + "," + Time.realtimeSinceStartup.ToString () + ",interactor," + (360f - transform.parent.transform.rotation.eulerAngles.x).ToString () + "," + (transform.parent.transform.rotation.eulerAngles.y).ToString() + ","+ transform.position.x.ToString () + "," + transform.position.y.ToString () + "," + transform.position.z.ToString () + "," + transform.position.z.ToString () + "," + transform.localPosition.x.ToString () + " , , , , ,start exited");
			c.gameObject.renderer.material.color = Color.yellow;
			Debug.Log("Start existed");
		}
		if (c.gameObject.name.StartsWith ("occluder")) {
			c.gameObject.renderer.material.color = Color.blue;
			Debug.Log("Occluder existed");
		}		
	}
	
	void Update () {

		GameObject target = GameObject.Find ("target");

		if (selectionTriggered && transform == target.transform.parent & dockingTriggered) {
			target.transform.parent = null;
			target.transform.position = target.transform.position+new Vector3(0,0f,10.5f);
			selectionTriggered = false;
		}

		if (!dockingTriggered ) {
			selectionTriggered = false;
		} else {
			if (transform == target.transform.parent) {
				target.transform.localEulerAngles = new Vector3 (target.transform.localRotation.eulerAngles.x, target.transform.localRotation.eulerAngles.y, (gyroZ*3)+targetZAngleOffset);
			}
		}
			
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
		/*
		if (selectionTriggered & !timeOfSelectionTriggerRegistered) {
			currentTimeAtSelectionTrigger = Time.realtimeSinceStartup;
			timeOfSelectionTriggerRegistered = true;
		}
		
		//if (Time.realtimeSinceStartup - currentTimeAtSelectionTrigger > 0.2f)
		//	selectionTriggered = false;
		
		if (currentTaskNo == noOfTasks) {
			Debug.Log ("Game over");
			Save ();
			calculateAndSaveResults ();
			Application.Quit();
		}
		else if (taskList[currentTaskNo].isCompleted()) {
			startSelected = false;
			Debug.Log ("Completed task: "+currentTaskNo.ToString());
			currentTaskNo++;
			Save ();
			if (currentTaskNo < noOfTasks ) taskList[currentTaskNo].start();
			else {
				calculateAndSaveResults ();
				Application.Quit();
			}
		}*/
		
		// This is the control part	
		float moveSpeed = 1;
		float damping = 3;
		bool recording = false;
		int startTime = 0;
		string data = "";
		
		if( oldGyroX != gyroX || oldGyroY != gyroY || oldGyroZ != gyroX) {
			if (Math.Abs (gyroY - oldGyroY) > 0.1f) gyroDrift += Math.Abs(gyroY-oldGyroY)*0.00085f;
			oldGyroX=gyroX;
			oldGyroY=gyroY;
			oldGyroZ=gyroZ;
		}
		float yaw = gyroY + gyroDrift;
		
		/*		if (oldTapX != tapX || oldTapY != tapY) {
			oldTapX = tapX;
			oldTapY = tapY;
			if (tapX == 100f) 	jsonLog.Add(taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",interactor," + (360f-transform.parent.transform.rotation.eulerAngles.x).ToString() + ","  + transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString() + "," + transform.localPosition.x.ToString()  + "," + transform.localPosition.y.ToString() + " , , , , ,cursor dropped");
		}
*/
		//depth cursor position
		transform.localPosition = new Vector3(0.0f,-1.0f,tapY);
		//ray origin position
		
		//ray rotation/angle
		//hideTarget ();
		hideOccluders (); 
		
		
		//float currentX = transform.parent.transform.position.x;
		//float currentY = transform.parent.transform.position.y;
		//float currentZ = transform.parent.transform.position.z;
		transform.parent.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(gyroX+20f, gyroY, 0), damping);
		
		/*transform.parent.transform.position = new Vector3(ir_x, ir_y*9/16, -16.5f);

		float reducedAlpha = gyroY/20 > -180 ? gyroY : -(360-(360+gyroY/20)); //- (float)Math.Atan (ir_x/3) * 57.2957795f;
		float reducedBeta = gyroX/10; //- (float)Math.Atan (ir_y/3) 1* 57.2957795f;

		transform.parent.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(reducedBeta-45*ir_y, reducedAlpha+73.5f*ir_x, 0.0f), damping);
*/
		//		display = string.Format(" ir_x={0}\n yaw = {1}\n reducedAlpha={2}\n\n ir_y = {3}\n gyroX = {4}\n reducedBeta={5}", ir_x, verticalAngle, currentX, ir_y, horizontalAngle, down);
		//		display = string.Format("Wiimote {0} IR values: ({1,7:G10}, {2,6:G10}) Maps to screen: ({3,4:G4}, {4,4:G4}),  Nunchuck stick: {5,5:G4}, {6,5:G4}", c, ir_x, ir_y);
		
		//		if (dragging) {
		//			renderer.material.color = Color.red;
		//		} else {
		//			renderer.material.color = Color.white;
		//		}
		
	}
	
	void hideOccluders() {
		float z = transform.position.z;
		for (int i = 1; i <= 42; i++) {
			GameObject occluder = GameObject.Find ("occluder"+i.ToString());
			
			/*
			float disX = (float) Math.Abs(transform.position.x-occluder.transform.position.x);
			float disY = (float) Math.Abs(transform.position.y-occluder.transform.position.y);
			float oZ = occluder.transform.position.z;
            */
			Color c = occluder.renderer.material.color;
			Color hidec = new Color (c.r, c.g, c.b, 0.0f);
			Color showc = new Color (c.r, c.g, c.b, 1.0f);
			
			//if (z > oZ && (disX < 0.3f || disY < 0.3f)) {
			if (isBetween(transform.position+new Vector3(0.0f,0.0f,-0.5f), occluder.transform.position,transform.parent.position, 0.5f)) {
				//show occluder
				//occluder.transform.position = new Vector3(100f,0.0f,0.0f);
				occluder.renderer.material.color = hidec;
				//display = string.Format(" a to m ={0}\n m to b = {1}\n a to b={2}\n", distance (transform.position, occluder.transform.position),distance (occluder.transform.position,transform.parent.position),distance (transform.position,transform.parent.position));
				
			} else {
				//hide occluder
				occluder.renderer.material.color = showc;
			} 
		}
	}
	
	bool isBetween(Vector3 a, Vector3 m, Vector3 b, float epsilon) {
		return  (-epsilon > (distance (a, m) + distance (m, b) - distance (a, b)) || ((distance (a, m) + distance (m, b) - distance (a, b)) < epsilon));
	}
	
	float distance (Vector3 a, Vector3 b) {
		return (float) Math.Sqrt ((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z));
	}
	
	void hideTarget() {
		
		GameObject target = GameObject.Find ("target"); 
		Vector3 pos = target.transform.position;
		float cor = 0.1f;
		float m = (360.0f-transform.parent.rotation.eulerAngles.x)* Mathf.Deg2Rad + 0.1f;
		float p = UnityEngine.Mathf.Atan2(pos.y,pos.z);
		//Debug.Log ("m = "+m.ToString()+" p = "+p.ToString());
		if (pos.x != 100f) {
			oldTargetX = pos.x;
		}
		if (m < p) {
			target.transform.position = new Vector3(100f, pos.y,pos.z);
		} else {
			//hide occluder
			target.transform.position = new Vector3(oldTargetX, pos.y,pos.z);
		}
	}
	
	
	void createTasks() {
		// add n random "select target tasks"
		
		for (int i = 0; i < noOfTasks/2; i++) {
			taskList.Add ((TargetingTask) new TargetingTask (i, 3f, 1.5f, 4000));
		}
		for (int i = noOfTasks/2; i < noOfTasks; i++) {
			taskList.Add ((TargetingTask) new TargetingTask (i, 1.5f, 0.75f, 4000));
		}
		
		//randomize order of tasks
		for (int i = 0; i < noOfTasks; i++) {
			Task temp = taskList[i];
			int randomIndex = UnityEngine.Random.Range (i,noOfTasks);
			taskList[i] = taskList[randomIndex];
			taskList[randomIndex] = temp;
		}
		
	}
	
	public void Save () {
		
		//System.IO.BinaryWriter bw = new System.IO.BinaryWriter(File.Open("Assets/test.json", FileMode.Create));
//		System.IO.File.WriteAllLines("c:/results/temp.txt",jsonLog.ToArray());
//		String content = System.IO.File.ReadAllText ("c:/results/temp.txt");
//		System.IO.File.AppendAllText ("c:/results/SmartCastingMonoOccluded" + sufix + ".csv", content);
		jsonLog.Clear ();
		
	}
	
	private void calculateAndSaveResults() {
		
		float temp = 0.0f;
		
		foreach (float e in selectionTimeLarge) {
			temp += e;
		}
		
		float average = temp/selectionTimeLarge.Count;
		
		temp = 0.0f;
		
		foreach (float e in selectionTimeLarge) {
			temp += Mathf.Pow((e - average),2);
		}
		
		float stdev = Mathf.Sqrt (temp / selectionTimeLarge.Count);
		
		Debug.Log ("Initial results Large: average = " + average.ToString () + ", stdev =" + stdev.ToString ());
		
		for (int i=0; i < selectionTimeLarge.Count; i++) {
			if (Math.Abs(selectionTimeLarge[i]-average) > 3*stdev)
				selectionTimeLarge.RemoveAt(i);
		}
		
		temp = 0.0f;
		
		foreach (float e in selectionTimeLarge) {
			temp += e;
		}
		
		average = temp/selectionTimeLarge.Count;
		
		temp = 0.0f;
		
		foreach (float e in selectionTimeLarge) {
			temp += Mathf.Pow((e - average),2);
		}
		
		stdev = Mathf.Sqrt (temp / selectionTimeLarge.Count);
		
		Debug.Log ("Final results Large: average = " + average.ToString () + ", stdev =" + stdev.ToString () +", count = "+selectionTimeLarge.Count);
		
//		System.IO.File.AppendAllText("c:/results/calculatedResultsLarge.txt","SmartcastingMonoOccluded,"+sufix+","+average.ToString()+","+stdev.ToString ()+","+selectionErrorLarge.ToString()+","+selectionTimeLarge.Count+Environment.NewLine);
		
		
		temp = 0.0f;
		
		foreach (float e in selectionTimeSmall) {
			temp += e;
		}
		
		average = temp/selectionTimeSmall.Count;
		
		temp = 0.0f;
		
		foreach (float e in selectionTimeSmall) {
			temp += Mathf.Pow((e - average),2);
		}
		
		stdev = Mathf.Sqrt (temp / selectionTimeSmall.Count);
		
		Debug.Log ("Initial results Small: average = " + average.ToString () + ", stdev =" + stdev.ToString ());
		
		for (int i=0; i < selectionTimeSmall.Count; i++) {
			if (Math.Abs(selectionTimeSmall[i]-average) > 3*stdev)
				selectionTimeSmall.RemoveAt(i);
		}
		
		temp = 0.0f;
		
		foreach (float e in selectionTimeSmall) {
			temp += e;
		}
		
		average = temp/selectionTimeSmall.Count;
		
		temp = 0.0f;
		
		foreach (float e in selectionTimeSmall) {
			temp += Mathf.Pow((e - average),2);
		}
		
		stdev = Mathf.Sqrt (temp / selectionTimeSmall.Count);
		
		Debug.Log ("Final results Small: average = " + average.ToString () + ", stdev =" + stdev.ToString () +", count = "+selectionTimeSmall.Count);
		
//		System.IO.File.AppendAllText("c:/results/calculatedResultsSmall.txt","SmartcastingMonoOccluded,"+sufix+","+average.ToString()+","+stdev.ToString ()+","+selectionErrorSmall.ToString ()+","+selectionTimeSmall.Count+Environment.NewLine);
		
	}			
}
