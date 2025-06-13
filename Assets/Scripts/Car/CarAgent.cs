using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class CarAgent : Agent
{
    float splitTimer = 0.00f;
    float crashingTimer = 0f;
    [SerializeField] private CarControl carControl;
    public GameObject checkpointHolder;
    CheckpointNum[] checkpoints;
    int checkpointNum;

    public void Start()
    {
        checkpoints = checkpointHolder.GetComponentsInChildren<CheckpointNum>();
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].num = i;
        }
        checkpointNum = -1;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carControl.rigidBody.linearVelocity);
        Vector3 dirToCheckpoint = checkpoints[(checkpointNum + 1)%checkpoints.Length].transform.position - transform.position;
        Vector2 dirToCheck2D = new Vector2(dirToCheckpoint.x, dirToCheckpoint.z);
        sensor.AddObservation(dirToCheck2D);
        sensor.AddObservation(Vector3.Dot(dirToCheckpoint, carControl.rigidBody.linearVelocity));
    }

    public override void OnEpisodeBegin()
    {
        carControl.reset();
        checkpointNum = -1;
        splitTimer = 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float turn = actions.ContinuousActions[0];
        float pedal = actions.ContinuousActions[1];
        carControl.Move(new Vector2(turn, pedal));
    }

    void FixedUpdate()
    {
        // Hard code the normal 50fps physics time for the split timer
        // splitTimer += 0.02f;

        // Reward based on how fast, subtracting 8 so that stationary vehicles are penalized
        AddReward((carControl.rigidBody.linearVelocity.magnitude-16)*0.02f);

        // Reward based on the distance to next checkpoint
        float dist = (
            checkpoints[(checkpointNum + 1)%checkpoints.Length].transform.position 
            - transform.position
        ).magnitude;
        int currentCheckpoint = checkpointNum; 
        if(checkpointNum == -1) currentCheckpoint = checkpoints.Length-1;
        float distBetweenWaypoints = (
            checkpoints[(checkpointNum + 1)%checkpoints.Length].transform.position 
            - checkpoints[currentCheckpoint].transform.position
        ).magnitude;

        if(dist > distBetweenWaypoints*0.9f) AddReward(-0.6f);
        else AddReward((distBetweenWaypoints-dist)*0.02f);

        // // Negative reward after too long on a split
        // if (splitTimer > 10)
        // {
        //     SetReward(-(splitTimer - 10) * 0.02f);
        // }
    }
    public void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")) {
            int num = other.GetComponent<CheckpointNum>().num;
            // Either we go up in checkpoint or we are wrapping around back to 0
            if (num == checkpointNum + 1 || (num == 0 && checkpointNum == checkpoints.Length - 1))
            {
                AddReward(400);
                splitTimer = 0f;
            }
            else
            {
                AddReward(-450);
                splitTimer = 0f;
                if(num == checkpoints.Length-1) EndEpisode();
            }
            checkpointNum = num;
        }
    }

    public void OnCollisionStay(Collision collision) {
        if(collision.collider.CompareTag("Wall")) {
            // Don't crash
            AddReward(-0.04f);
            crashingTimer += 0.02f;
        }
        if(crashingTimer > 8) EndEpisode();
    }

    public void OollisionExit(Collision collision)
    {
        if(collision.collider.CompareTag("Wall")) {
            AddReward(0.04f);
            crashingTimer = 0f;
        }
    }
}
