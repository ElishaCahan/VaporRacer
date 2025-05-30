using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class CarAgent : Agent
{
    int check = -1;
    int formercheck = -1;
    private float time = 0.00f;
    [SerializeField] private CarControl carControl;
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carControl.rigidBody.linearVelocity);
    }

    public override void OnEpisodeBegin()
    {
        carControl.reset();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float turn = actions.ContinuousActions[0];
        float pedal = actions.ContinuousActions[1];
        carControl.Move(new Vector2(turn, pedal));
    }
    void Update()
    {
        time += Time.deltaTime;
        if(time > 10)
        {
            AddReward(carControl.rigidBody.linearVelocity.magnitude*Time.deltaTime);
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
    }
}
