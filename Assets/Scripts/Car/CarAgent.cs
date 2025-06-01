using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class CarAgent : Agent
{
    int check = -1;
    int formercheck = -1;
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
        check = -1;
        formercheck = -1;
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
            AddReward(avgSpeedDuringTime-10);
            avgSpeedDuringTime = 0;
        }
        if(splitTimer > 10) {
            SetReward(-5*(splitTimer-10));
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        formercheck = check;
        check = other.GetComponent<CheckpointNum>().num;
        if (formercheck >= check)
        {
            Debug.Log("Penalty Triggered: " + check);
            SetReward(0);
        }
        if(check > formercheck)
            splitTimer = 0;
    }
}
