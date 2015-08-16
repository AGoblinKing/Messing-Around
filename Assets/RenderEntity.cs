using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Json;

public struct EntityComponent {
	public JsonObject data;
	public GameObject gameObject;

	public EntityComponent(JsonObject p1, GameObject p2) {
		data = p1;
		gameObject = p2;
	}
}

public class RenderEntity : MonoBehaviour {
	public TextAsset entityDef;
	public GameObject nodeType;

	private	JsonObject data;
	private Dictionary<string, EntityComponent> euids = new Dictionary<string, EntityComponent>();

	// Use this for initialization
	void Start () {
		data = (JsonObject)JsonValue.Parse (entityDef.ToString ());	

		// consider making these joints
		foreach(JsonObject node in data["nodes"]) {
			GameObject obj = (GameObject)Object.Instantiate(nodeType, 
			                                    new Vector3(node["pos"][0], node["pos"][1], node["pos"][2]), 
			                                    new Quaternion());

			obj.transform.parent = transform;
			euids[node["euid"]] = new EntityComponent(node, obj);
		}

		// Yay add sprite data
		foreach (JsonObject bone in data["bones"]) {
			EntityComponent from = euids[bone["from"]];
			EntityComponent to = euids[bone["to"]];

			GameObject boneObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			boneObject.transform.position = from.gameObject.transform.position;			                                                                
			boneObject.transform.LookAt(to.gameObject.transform.position);
			boneObject.transform.localScale.Set(1, 1, Vector3.Distance(from.gameObject.transform.position, to.gameObject.transform.position));
			boneObject.transform.parent = transform;

			euids[bone["euid"]] = new EntityComponent(bone, boneObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
