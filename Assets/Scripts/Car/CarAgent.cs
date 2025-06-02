using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class CarAgent : Agent
{
    int check = -2;
    int formercheck = -2;
    float speedRewardTimer = 0.00f;
    float splitTimer = 0.00f;
    float avgSpeedDuringTime = 0;
    [SerializeField] private CarControl carControl;
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carControl.rigidBody.linearVelocity);
    }

    public override void OnEpisodeBegin()
    {
        carControl.reset();
        check = -2;
        formercheck = -2;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float turn = actions.ContinuousActions[0];
        float pedal = actions.ContinuousActions[1];
        carControl.Move(new Vector2(turn, pedal));
    }

    void Update()
    {
        avgSpeedDuringTime = (avgSpeedDuringTime*speedRewardTimer + carControl.rigidBody.linearVelocity.magnitude)/(speedRewardTimer+Time.deltaTime);
        speedRewardTimer += Time.deltaTime;
        splitTimer += Time.deltaTime;
        if(speedRewardTimer > 3)
        {
            AddReward((avgSpeedDuringTime-30)*2);
            avgSpeedDuringTime = 0;
            speedRewardTimer = 0;
        }
        if(splitTimer > 20) {
            SetReward(-5*(splitTimer-10)*Time.deltaTime);
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Checkpoint")) return;
        formercheck = check;
        check = other.GetComponent<CheckpointNum>().num;
        if (formercheck >= check)
        {
            Debug.Log("Penalty Triggered: " + check);
            SetReward(-100);
        }
        if(check > formercheck) {
            splitTimer = 0;
            AddReward(60);
            if(formercheck == 12 && check == 13) EndEpisode();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        // if(collision.body.CompareTag("Wall")) {
        //     AddReward(-20);
        // }
    }
}
