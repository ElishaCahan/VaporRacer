using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class CarAgent : Agent
{
    int check = -1;
    int formerCheck = -1;
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
        formerCheck = -1;
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
            AddReward((avgSpeedDuringTime-25)*2.5f);
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
        formerCheck = check;
        check = other.GetComponent<CheckpointNum>().num;

        if(formerCheck == -1 && check > 2) {
            Debug.Log("Penalty Triggered: " + check);
            SetReward(-100);
        }
        if (formerCheck != -1 && formerCheck >= check)
        {
            Debug.Log("Penalty Triggered: " + check);
            SetReward(-100);
        }
        if(check > formerCheck) {
            splitTimer = 0;
            AddReward(300);
            if(formerCheck == 13 && check == 14) {EndEpisode();}
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Wall")) {
            AddReward(-80);
        }
    }
}
