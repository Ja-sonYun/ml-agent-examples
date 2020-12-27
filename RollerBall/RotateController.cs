using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;                           // Import unity modules
using Unity.MLAgents.Sensors;                   // Import unity sensors for observation

public class RotateController : Agent
{
    Vector3 moved;                              // store actions here
    public GameObject ball;
    public Rigidbody ballBody;
    Quaternion ObjAngle = Quaternion.identity;  // use quaterion for rotate floor

    public override void Initialize()
    {
        Debug.Log("init");
        this.ballBody = this.ball.GetComponent<Rigidbody>(); // add gravity to the ball

        ObjAngle.eulerAngles = Vector3.zero;                 // reset floor angle

        Vector3 FirstAction = Vector3.zero;
        FirstAction[0] = Random.Range(-2f, 2f);
        FirstAction[1] = Random.Range(-2f, 2f);
        FirstAction[2] = Random.Range(-2f, 2f);
        ballBody.velocity = FirstAction;                     // start with these parameters. (random speed)
    }

    public override void OnEpisodeBegin()                    // this parents function will executed when an episode begin. use this as an initializer of the environment
    {
        Debug.Log("episode");
        if(this.ball.transform.localPosition.y < 0)          // reset all when the ball.y was reach 0
        {

            this.ball.transform.localPosition = new Vector3(Random.Range(-6.0f, 6.0f), 8.0f, Random.Range(-6.0f, 6.0f)); // reset position with random starting coordinates
            this.ballBody.velocity = Vector3.zero;        // ?? TODO: unnecessary line
            this.ballBody.angularVelocity = Vector3.zero; // reset acceleration

            ObjAngle = Quaternion.identity;               // reset rotation

            Vector3 FirstAction = Vector3.zero;
            FirstAction[0] = Random.Range(-2f, 2f);
            FirstAction[1] = Random.Range(-2f, 2f);
            FirstAction[2] = Random.Range(-2f, 2f);
            this.ballBody.velocity = FirstAction;         // give random speed to the ball (but not that fast between -2f, 2f)

        }
    }

    public override void CollectObservations(VectorSensor sensor)  // collect parameters from this function. this function is connected with a component named
                                                                   // 'Behavior Parameters'.
    {
        sensor.AddObservation(this.transform.rotation[0]); // this has one ( x coordinate )
        sensor.AddObservation(this.transform.rotation[2]); // and this too. ( z coordinate )
        sensor.AddObservation(this.ball.transform.localPosition); // this is a vector that have three.
        sensor.AddObservation(this.ballBody.velocity);            // and this too.
        Debug.Log("observed");
        // 8 parameters that I added at above will be observed.
        // The value of the Unity->Floor->Inspector->Behavior Parameters->Vector Observation->Stacked Vectors should same like this.
    }

    public override void OnActionReceived(float[] vectorAction)   // When this agent got action, this function will be called.
    {
        moved = Vector3.zero;
        Debug.Log("OnAction");
        int action = (int)vectorAction[0]; // type of vectorAction is a float. make it as an int. You can edit this vectorActions from Unity. Default value is 0~1(not sure)
        // To change range of vectorActions, Go to the setting of Behavior Parameters that I mentioned at line 55. Switch Vector Action->Space Type to Discrete and modify value
        // of 'Branch * size' to the new value. I set 5, so I can get value between 0~5
        if(action == 1) moved[0] = this.transform.rotation.x + 50;
        if(action == 2) moved[0] = this.transform.rotation.x - 50;
        if(action == 3) moved[2] = this.transform.rotation.x + 50;
        if(action == 4) moved[2] = this.transform.rotation.x - 50; // actions
        ObjAngle.eulerAngles = moved;                              // actions

        transform.rotation = Quaternion.Slerp(this.transform.rotation, this.ObjAngle, Time.deltaTime * 2.0f); // moving

        if(this.ball.transform.localPosition.y < 0) // when failed,
        {
            SetReward(-1f);                         // lose reward.
            EndEpisode();
        } else {
            SetReward(0.1f);                        // or get reward
        }
    }

    public override void Heuristic(float[] actionsOut) // Human control
    {
        actionsOut[0] = 0;
        if(Input.GetKey(KeyCode.LeftArrow)) actionsOut[0] = 1; // left action
        if(Input.GetKey(KeyCode.RightArrow)) actionsOut[0] = 2;// right action
        if(Input.GetKey(KeyCode.UpArrow)) actionsOut[0] = 3;   // up action
        if(Input.GetKey(KeyCode.DownArrow)) actionsOut[0] = 4; // down action
    }

}
