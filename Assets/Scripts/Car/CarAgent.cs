using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class CarAgent : Agent
{
    [SerializeField] private CarControl carControl;
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carControl.rigidBody.linearVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float turn = actions.ContinuousActions[0];
        float pedal = actions.ContinuousActions[1];
        carControl.Move(new Vector2(turn, pedal));
    }
}
