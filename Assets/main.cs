using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System;
using SocketIOClient;
using System.IO;

public class main : MonoBehaviour {

	public List<string> jsonLog;
	public List<Task> taskList;
	public int currentTaskNo = 0;
	public const int noOfTasks = 20;
	public float logFrequency = 0.2f;
	public InteractorState interactorState = new InteractorState();
	public TargetState targetState = new TargetState();
	public InteractionPlaneState interactionPlaneState = new InteractionPlaneState();
	private const string ARRAY_FLAG = "[";
	private float gyroX = 0.0f;
	private float gyroY = 0.0f;
	private float gyroZ = 0.0f;
	private float oldAccelX = 0.0f;
	private float oldAccelY = 0.0f;
	private float accelX = 0.0f;
	private float accelY = 0.0f;
	private float accelZ = 0.0f;
	private float oldGyroX = 0.0f;
	private float oldGyroY = 0.0f;
	private float oldGyroZ = 0.0f;
	private float tapX = 0.0f;
	private float tapY = 0.0f;
	private float oldTapX = 0.0f;
	private float oldTapY = 0.0f;
	private Client client = new Client("http://192.168.1.122:8091");//new Client("http://24.246.24.104:8091"); 
	private bool dragging = false;
	private Color originalColor;
	private static GUIText text;

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

		GameObject target = GameObject.Find("target");
		public Vector3 targetInit;

		public TargetingTask(int taskNo, Vector3 initialPosition) {
			no = taskNo;
			name = "Select sphere";
			targetInit = initialPosition;
		}

		public override void start(){
			text.text = "Task " + (no + 1).ToString () + ". " + name;
			target.transform.position = targetInit;
		}

		public override bool isCompleted(){
			if (target.renderer.material.color == Color.red) { 
				completed = true;
			}
			return completed;
		}
	}

	public class TranslatingTask : Task {

		GameObject target = GameObject.Find("target");
		GameObject destination = GameObject.Find("destination");
		public Vector3 targetInit;
		public Vector3 destinationInit;

		public TranslatingTask(int taskNo, Vector3 initialPosition, Vector3 destinationInitialPosition) {
			no = taskNo;
			name = "Move sphere into the box";
			targetInit = initialPosition;
			destinationInit = destinationInitialPosition;
		}

		public override void start(){
			text.text = "Task " + no.ToString () + ". " + name;
			target.transform.position = targetInit;
			destination.transform.position = destinationInit;
			destination.renderer.material.color = Color.white;

		}
		
		public override bool isCompleted(){
			if (target.renderer.material.color == Color.red && destination.renderer.material.color == Color. green) { 
				completed = true;
				//waitNSec (2);
			}
			return completed;
		}

	}

	public class PathTask: Task {
	
		public override void start(){
		}
		
		public override bool isCompleted(){
			return completed;
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
		object ax = null;
		object ay = null;
		object az = null;
		object mx = null;
		object my = null;
		
		if (obj != null) {
			if (obj.TryGetValue ("name", out name)) {
				if (name != null) {
					if (name.ToString().Equals("gyro")) {
						
						obj.TryGetValue ("args", out args);
						
						JsonArray jsonArray = (JsonArray) SimpleJson.SimpleJson.DeserializeObject(args.ToString());
						JsonObject gyroValues = (JsonObject) SimpleJson.SimpleJson.DeserializeObject(jsonArray[0].ToString());


						gyroValues.TryGetValue("alpha", out alpha);
						gyroY = -float.Parse (alpha.ToString());

						gyroValues.TryGetValue("beta", out beta);
						gyroX = -float.Parse (beta.ToString());
						
						gyroValues.TryGetValue("gamma", out gamma);
						gyroZ = -float.Parse (gamma.ToString());
						
						
					}

					if (name.ToString().Equals("accel")) {
						
						obj.TryGetValue ("args", out args);
						
						JsonArray jsonArray = (JsonArray) SimpleJson.SimpleJson.DeserializeObject(args.ToString());
						JsonObject accelValues = (JsonObject) SimpleJson.SimpleJson.DeserializeObject(jsonArray[0].ToString());
						
						
						accelValues.TryGetValue("x", out ax);
						float tempX = float.Parse (ax.ToString());
						//if (tempX > 0.2f || tempX < -0.2f) print (tempX);
						accelX = (tempX > 0.002f || tempX < -0.002f) ? tempX : 0.0f;
						
						accelValues.TryGetValue("y", out ay);
						float tempY = float.Parse (ay.ToString());
						//if (tempY > 0.2f || tempY < -0.2f) print (tempY);
						accelY = (tempY > 0.002f || tempY < -0.002f) ? tempY : 0.0f;

						accelValues.TryGetValue("z", out az);
						accelZ = float.Parse (az.ToString());
						
						
					}

					if (name.ToString().Equals("tapstart") || name.ToString().Equals("tapmove")) {
						obj.TryGetValue ("args", out args);
						
						JsonArray jsonArray = (JsonArray) SimpleJson.SimpleJson.DeserializeObject(args.ToString());
						JsonObject touchStartValues = (JsonObject) SimpleJson.SimpleJson.DeserializeObject(jsonArray[0].ToString());
						
						touchStartValues.TryGetValue("mx", out mx);
						tapX = (float) float.Parse (mx.ToString());
						
						touchStartValues.TryGetValue("my", out my);
						tapY = (float) float.Parse (my.ToString());
						
						dragging = true;
						
						//Debug.Log (tapX.ToString()+","+tapY.ToString());
					}
					
					
					if (name.ToString().Equals("tapend")) {
						
						dragging = false;
						tapX = 0.0f;
						tapY = 0.0f;
					}
					
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

		text = (GUIText) GameObject.Find ("instructions").guiText;
		transform.parent.transform.RotateAround(Vector3.zero,Vector3.left,-45);
		client.Opened += SocketOpened;
		client.Message += SocketMessage;
		client.SocketConnectionClosed += SocketConnectionClosed;
		client.Error +=SocketError;
		client.Connect();
		//print (client.IsConnected);

		jsonLog = new List<string> ();
		taskList = new List<Task> ();

		// add tasks
		createTasks();

		// start with first task
		taskList[currentTaskNo].start();

		// log everything
		InvokeRepeating ("LogObjects", 0, logFrequency); 

	}

	void LogObjects() {

		float currentTime = Time.realtimeSinceStartup;

		GameObject interactionPlane = GameObject.Find("interactionPlane");	
		GameObject target = GameObject.Find("target");		
		GameObject interactor = GameObject.Find("interactor");		

		if (target.transform.position.x != targetState.x || target.transform.position.y != targetState.y || target.transform.position.z != targetState.z) {
			targetState.z = target.transform.position.z;
			targetState.y = target.transform.position.y;
			targetState.x = target.transform.position.x;
			targetState.taskId = taskList[currentTaskNo].no;
			targetState.timestamp = currentTime;
			targetState.name="target";
			targetState.color = target.renderer.material.color;
			jsonLog.Add(targetState.taskId.ToString()+ "," + targetState.timestamp.ToString()+ "," +targetState.name.ToString()+ "," +targetState.color.ToString()+ "," +targetState.x.ToString()+ "," +targetState.y.ToString()+ "," +targetState.z.ToString());
		}

		if (interactor.transform.position.x != interactorState.x || interactor.transform.position.y != interactorState.y || interactor.transform.position.z != interactorState.z) {
			interactorState.z = interactor.transform.position.z;
			interactorState.y = interactor.transform.position.y;
			interactorState.x = interactor.transform.position.x;
			interactorState.localZ = interactor.transform.localPosition.z;
			interactorState.localY = interactor.transform.localPosition.y;
			interactorState.localX = interactor.transform.localPosition.x;
			interactorState.taskId = taskList[currentTaskNo].no;
			interactorState.timestamp = currentTime;
			interactorState.name="interactor";
			interactorState.color=interactor.renderer.material.color;
			jsonLog.Add(interactorState.taskId.ToString() + "," + interactorState.timestamp.ToString()+ "," + interactorState.name.ToString() + "," + interactorState.color.ToString() + "," + interactorState.x.ToString() + "," + interactorState.y.ToString() + "," + interactorState.z.ToString() + "," + interactorState.localX.ToString() + "," + interactorState.localY.ToString() + "," + interactorState.localZ.ToString());
		}

		if (interactionPlane.transform.position.x != interactionPlaneState.x || interactionPlane.transform.position.y != interactionPlaneState.y || interactionPlane.transform.position.z != interactionPlaneState.z ||
		    interactionPlane.transform.rotation.x != interactionPlaneState.rx || interactionPlane.transform.rotation.y != interactionPlaneState.ry || interactionPlane.transform.rotation.z != interactionPlaneState.rz) {
			interactionPlaneState.z = interactionPlane.transform.position.z;
			interactionPlaneState.y = interactionPlane.transform.position.y;
			interactionPlaneState.x = interactionPlane.transform.position.x;
			interactionPlaneState.rz = interactionPlane.transform.rotation.z;
			interactionPlaneState.ry = interactionPlane.transform.rotation.y;
			interactionPlaneState.rx = interactionPlane.transform.rotation.x;
			interactionPlaneState.taskId = taskList[currentTaskNo].no;
			interactionPlaneState.timestamp = currentTime;
			interactionPlaneState.name="interactionPlane";
			
			jsonLog.Add(interactionPlaneState.taskId.ToString() + "," + interactionPlaneState.timestamp.ToString()+ "," + interactionPlaneState.name+ "," + interactorState.color.ToString() + "," + interactionPlaneState.x.ToString()+ "," + interactionPlaneState.y.ToString()+ "," + interactionPlaneState.z.ToString()+ ", , , ," + interactionPlaneState.rx.ToString()+ "," + interactionPlaneState.ry.ToString()+ "," + interactionPlaneState.rz.ToString());	
		}

	}
	
	void OnCollisionEnter(Collision c)
	{
		// if dragged to destination, change destination color
		if (c.gameObject.name == "destination" && renderer.material.color == Color.red) {
			jsonLog.Add (taskList [currentTaskNo].no.ToString () + "," + Time.realtimeSinceStartup.ToString () + ",destination," + c.gameObject.renderer.material.color.ToString () + "," + c.gameObject.transform.position.x.ToString () + "," + c.gameObject.transform.position.y.ToString () + "," + c.gameObject.transform.position.z.ToString () + ", , , , , , , target at destination");
			jsonLog.Add (taskList [currentTaskNo].no.ToString () + "," + Time.realtimeSinceStartup.ToString () + ",interactor," + renderer.material.color.ToString () + "," + transform.position.x.ToString () + "," + transform.position.y.ToString () + "," + transform.position.z.ToString () + ", , , , , , , target at destination");

			c.gameObject.renderer.material.color = Color.green;
		}
		//if selected, change target and interactor color
		if (c.gameObject.name == "target") {
			jsonLog.Add(taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",target," + c.gameObject.renderer.material.color.ToString() + "," + c.gameObject.transform.position.x.ToString() + "," + c.gameObject.transform.position.y.ToString() + "," + c.gameObject.transform.position.z.ToString() + ", , , , , , , target selected");
			jsonLog.Add(taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",interactor," + renderer.material.color.ToString() + "," + transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString() + ", , , , , , , target selected");
			originalColor = c.gameObject.renderer.material.color;
			c.gameObject.renderer.material.color = Color.red;
			renderer.material.color = Color.red;
		}

	}

	void OnCollisionStay(Collision c)
	{
		if (c.gameObject.name == "target") {
			jsonLog.Add(taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",target," + c.gameObject.renderer.material.color.ToString() + "," + c.gameObject.transform.position.x.ToString() + "," + c.gameObject.transform.position.y.ToString() + "," + c.gameObject.transform.position.z.ToString() + ", , , , , , , target in move");
			jsonLog.Add(taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",interactor," + renderer.material.color.ToString() + "," + transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString() + ", , , , , , , target in move");
			c.gameObject.transform.position = Vector3.Lerp(c.gameObject.transform.position, transform.position, Time.deltaTime * 14f);;
			c.gameObject.renderer.material.color = Color.red;
			renderer.material.color = Color.red;
		}
		
	}

	void OnCollisionExit(Collision c)
	{
		if (c.gameObject.name == "target") {
			c.gameObject.transform.position = transform.position;
			jsonLog.Add(taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",target," + c.gameObject.renderer.material.color.ToString() + "," + c.gameObject.transform.position.x.ToString() + "," + c.gameObject.transform.position.y.ToString() + "," + c.gameObject.transform.position.z.ToString() + ", , , , , , , target dropped");
			jsonLog.Add(taskList[currentTaskNo].no.ToString() + "," + Time.realtimeSinceStartup.ToString()+ ",interactor," + renderer.material.color.ToString() + "," + transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString() + ", , , , , , , target dropped");
			c.gameObject.renderer.material.color = originalColor;
			renderer.material.color = Color.yellow;
			Save ();
		}
		
	}
	
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
		// if task is completed go to next task, if any
		if (currentTaskNo == noOfTasks) {
			Debug.Log ("Game over");
			Save ();
			Application.Quit();
		}
		else if (taskList[currentTaskNo].isCompleted()) {
				Debug.Log ("Completed task: "+currentTaskNo.ToString());
				currentTaskNo++;
				taskList[currentTaskNo].start();
		}

		// This is the control part	
		float moveSpeed = 1;
		float damping = 3;
		bool recording = false;
		int startTime = 0;
		string data = "";
		
		if( oldGyroX != gyroX || oldGyroY != gyroY || oldGyroZ != gyroX) {
			oldGyroX=gyroX;
			oldGyroY=gyroY;
			oldGyroZ=gyroZ;
		}
		
		if (oldTapX != tapX || oldTapY != tapY) {
			oldTapX = tapX;
			oldTapY = tapY;
		}

		
		if (oldAccelX != accelX || oldAccelY != accelY) {
			oldAccelX = accelX;
			oldAccelY = accelY;
			//transform.parent.rigidbody.AddForce(new Vector3(accelX/100f, accelY/100f, 0.0f)*Time.deltaTime,ForceMode.Acceleration);
		}

		float tempX = 0f;
		float tempY = 0f;
		if (accelX > 0) 
			tempX = (transform.parent.transform.position.x < 2.35f)  ? tempX = accelX : tempX = 0.0f;
		else 
			tempX = (transform.parent.transform.position.x > -2.35f) ? tempX = accelX : tempX = 0.0f;
		
		if (accelY > 0)
			tempY = (transform.parent.transform.position.y < 1.7f)  ? tempY = 1/2*accelY*100 : tempY = 0.0f;
		else 
			tempY = (transform.parent.transform.position.y > -1.7f) ? tempY = 1/2*accelY*100 : tempY = 0.0f;

		//transform.parent.rigidbody.AddForce(new Vector3(tempX/100f, tempY/100f, 0.0f),ForceMode.Impulse);
		transform.parent.transform.position = transform.parent.position + new Vector3((float) tempY/10, tempX/10,0.0f); //Quaternion.Lerp(transform.parent.position, Quaternion.Euler(accelX/100, 0.0f, 0.0f), damping);

		transform.parent.transform.localRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(gyroX, gyroY, gyroZ), damping);
		transform.localPosition = new Vector3((float)(tapX - 0.5)+0.001f,0.001f,(float)(1 - tapY*0.95)+0.001f);
		
		if (dragging) {
			renderer.material.color = Color.yellow;
		} else {
			renderer.material.color = Color.white;
		}

	}

	void createTasks() {
		
		// add n random "select target tasks"
		for (int i = 1; i < 20; i=i+4) {
			taskList.Add ((TranslatingTask) new TranslatingTask (i, new Vector3(UnityEngine.Random.Range (0.0f,8.0f),UnityEngine.Random.Range (2.0f,5.0f),UnityEngine.Random.Range (4.0f,6.0f)), 
			                                                     new Vector3(UnityEngine.Random.Range (-8.0f,0.0f),UnityEngine.Random.Range (4.0f,6.0f),UnityEngine.Random.Range (1.0f,4.0f))));
			taskList.Add ((TranslatingTask) new TranslatingTask (i+1, new Vector3(UnityEngine.Random.Range (0.0f,8.0f),UnityEngine.Random.Range (3.0f,6.0f),UnityEngine.Random.Range (1.0f,6.0f)), 
			                                                     new Vector3(UnityEngine.Random.Range (-8.0f,0.0f),UnityEngine.Random.Range (1.0f,5.0f),UnityEngine.Random.Range (1.0f,5.0f))));
			taskList.Add ((TranslatingTask) new TranslatingTask (i+2, new Vector3(UnityEngine.Random.Range (-8.0f,0.0f),UnityEngine.Random.Range (2.0f,5.0f),UnityEngine.Random.Range (1.0f,5.0f)), 
			                                                     new Vector3(UnityEngine.Random.Range (0.0f,8.0f),UnityEngine.Random.Range (3.0f,6.0f),UnityEngine.Random.Range (1.0f,6.0f))));
			taskList.Add ((TranslatingTask) new TranslatingTask (i+3, new Vector3(UnityEngine.Random.Range (-8.0f,0.0f),UnityEngine.Random.Range (3.0f,6.0f),UnityEngine.Random.Range (1.0f,5.0f)), 
			                                                     new Vector3(UnityEngine.Random.Range (0.0f,8.0f),UnityEngine.Random.Range (1.0f,5.0f),UnityEngine.Random.Range (1.0f,6.0f))));
		}

	}

	public void Save () {
		
		//System.IO.BinaryWriter bw = new System.IO.BinaryWriter(File.Open("Assets/test.json", FileMode.Create));
//		System.IO.File.WriteAllLines("e:/results"+UnityEngine.Random.Range(0.0f,1.0f).ToString()+".csv",jsonLog.ToArray());

	}

}
