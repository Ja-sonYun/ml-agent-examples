using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Unity.MLAgents;

public class AgentC : Agent
{
    public GameObject wallsObj;
    public GameObject CHECKPOINT;
    public Transform checkPoints;
    public Vector3 startPoint;
    Rigidbody rBody;
    int lastCheckpoint;
    bool hitWall;
    bool hitCheckpoint;

    void OnCollisionEnter(Collision collision) {
        hitWall = true;
    }

    void OnTriggerEnter(Collider checkpoint)
    {
        hitCheckpoint = true;
        string thisCheckpoint_s = Regex.Replace(checkpoint.name, "[^0-9]", "");
        this.lastCheckpoint = (thisCheckpoint_s == "") ? 0 : int.Parse(thisCheckpoint_s);
    }

    public override void Initialize()
    {
        this.rBody = GetComponent<Rigidbody>();
        this.hitWall = false;
        this.lastCheckpoint = -1;
    }

    public override void OnEpisodeBegin()
    {
        if(transform.localPosition.y > 8 || hitWall) {
            this.hitWall = false;
            this.lastCheckpoint = -1;

            // this.transform.localPosition = new Vector3(-70.0f, 6f, -0.4f);
            this.transform.localPosition = this.startPoint;
            this.transform.rotation = Quaternion.identity;
            this.rBody.velocity = Vector3.zero;
            this.rBody.angularVelocity = Vector3.zero;
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int HeadingTo = -1;
        Vector3 coordinatesOfCurrentGoal = Vector3.zero;
        foreach(Transform checkpoint in checkPoints)
        {
            string thisCheckpoint_s = Regex.Replace(checkpoint.name, "[^0-9]", "");
            int thisCheckpoint = (thisCheckpoint_s == "") ? 0 : int.Parse(thisCheckpoint_s);

            if(this.lastCheckpoint + 1 == thisCheckpoint)
            {
                HeadingTo = thisCheckpoint;
                coordinatesOfCurrentGoal = checkpoint.position;
                break;
            }
        }
        Vector3 direction = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        int action = (int)vectorAction[0];
        if(action == 1) direction = transform.forward;
        if(action == 2) rotation = transform.up;
        if(action == 3) rotation = transform.up * -1.0f;

        this.transform.Rotate(rotation, Time.deltaTime * 150f);
        this.rBody.AddForce(direction * 0.5f, ForceMode.VelocityChange);
        // Debug.Log(Vector3.Distance(this.transform.position, coordinatesOfCurrentGoal).ToString());

        if(transform.localPosition.y > 8 || hitWall) {
            // Debug.Log(HeadingTo.ToString());
            SetReward(-1f);
            EndEpisode();
        } else if(this.hitCheckpoint) {
            // Debug.Log("------------" + HeadingTo.ToString() + "------------");
            AddReward(0.1f);
            this.hitCheckpoint = false;
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        if(Input.GetKey(KeyCode.UpArrow)) actionsOut[0] = 1;
        if(Input.GetKey(KeyCode.LeftArrow)) actionsOut[0] = 2;
        if(Input.GetKey(KeyCode.RightArrow)) actionsOut[0] = 3;
    }
}
