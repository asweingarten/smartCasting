#pragma strict
var movTexture : MovieTexture;
function Start () {
   renderer.material.mainTexture = movTexture;
   movTexture.Play();
}

function Update () {

}