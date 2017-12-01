using UnityEngine;

public class BicycleController : MonoBehaviour {
    public WheelCollider frontWheelCollider;
    public WheelCollider rearWheelCollider;
    public Rigidbody rigidBody;
    public GameObject frontWheelMesh;
    public GameObject rearWheelMesh;
    public GameObject frontAssembly;
    public GameObject road;

    public AudioClip tireAudioRoadClip;
    public AudioClip tireAudioDirtClip;
    public AudioClip pedalingAudioClip;
    public AudioClip idleAudioClip;

    public float pedalForce = 100f;
    public float brakeForce = 100f;
    public float maxLeanAngle = 20f;
    public float maxTurnAngle = 10f;

    protected float powerInput;
    protected float steeringInput;
    protected bool frontBrake;
    protected bool rearBrake;
    protected float turningAngle;
    protected float previousDesiredZRot;

    protected AudioSource tireAudioRoad;
    protected AudioSource tireAudioDirt;
    protected AudioSource pedalingAudio;
    protected AudioSource idleAudio;

    // Use this for initialization
    void Awake() {
        powerInput = 0;
        steeringInput = 0;
        frontBrake = false;
        rearBrake = false;
        previousDesiredZRot = 0;

        tireAudioRoad = SetUpAudioSource(tireAudioRoadClip);
        tireAudioDirt = SetUpAudioSource(tireAudioDirtClip);
        pedalingAudio = SetUpAudioSource(pedalingAudioClip);
        idleAudio = SetUpAudioSource(idleAudioClip);
    }

    public void Reset() {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.isKinematic = true;
        rigidBody.isKinematic = false;
        Vector3 frontRotation = frontAssembly.transform.localRotation.eulerAngles;
        frontRotation.z = 0;
        frontAssembly.transform.localRotation = Quaternion.Euler(frontRotation);
    }

    // Update is called once per frame
    protected virtual void Update() {
        getInputs();
        rotateWheels();
        rotateFrontAssembly();
        ApplyRollingAudio();
        ApplyRidingAudio();
    }

    protected void FixedUpdate() {
        applyWheelTorques();

        float forwardSpeed = transform.InverseTransformDirection(rigidBody.velocity).z;
        float desiredZRot = steeringInput * (forwardSpeed * forwardSpeed) * -0.2f;
        desiredZRot = Mathf.Clamp(desiredZRot, -maxLeanAngle, maxLeanAngle);
        desiredZRot = Mathf.Lerp(previousDesiredZRot, desiredZRot, Time.fixedDeltaTime * 5);
        previousDesiredZRot = desiredZRot;

        Vector3 eulerRot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRot.x, eulerRot.y, desiredZRot);
    }

    protected virtual void getInputs() {
    }

    protected void rotateWheels() {
        float rearColliderRotation = rearWheelCollider.rpm;
        float frontColliderRotation = frontWheelCollider.rpm;
        rearWheelMesh.transform.Rotate(0, rearColliderRotation * -6 * Time.deltaTime, 0);
        frontWheelMesh.transform.Rotate(0, frontColliderRotation * -6 * Time.deltaTime, 0);
    }

    protected void rotateFrontAssembly() {
        Vector3 frontRotation = frontAssembly.transform.localRotation.eulerAngles;
        frontRotation.z = 0;
        frontAssembly.transform.localRotation = Quaternion.Euler(frontRotation + new Vector3(0, 0, turningAngle));
    }

    protected void applyWheelTorques() {
        rearWheelCollider.motorTorque = powerInput * pedalForce;
        frontWheelCollider.brakeTorque = frontBrake ? brakeForce : (rearBrake ? brakeForce * 0.05f : 0);
        rearWheelCollider.brakeTorque = rearBrake ? brakeForce : (frontBrake ? brakeForce * 0.05f : 0);
        frontWheelCollider.steerAngle = turningAngle;
    }

    protected bool IsOnRoad() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit)) {
            return hit.transform.tag == "Road";
        }
        return false;
    }

    // sets up and adds new audio source to the gane object
    protected AudioSource SetUpAudioSource(AudioClip clip) {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 1;
        source.loop = true;

        // start the clip from a random point
        source.time = Random.Range(0f, clip.length);
        source.minDistance = 5;
        source.maxDistance = 200;
        source.dopplerLevel = 0;
        return source;
    }

    protected void ApplyRollingAudio() {
        if (IsOnRoad()) {
            if (!tireAudioRoad.isPlaying)
                tireAudioRoad.Play();
            tireAudioDirt.Stop();
        }
        else {
            if (!tireAudioDirt.isPlaying)
                tireAudioDirt.Play();
            tireAudioRoad.Stop();
        }

        float vel = rigidBody.velocity.magnitude;
        tireAudioRoad.pitch = vel * 0.15f;
        tireAudioDirt.pitch = vel * 0.15f;
    }

    protected void ApplyRidingAudio() {
        if (powerInput > 0) {
            if (!pedalingAudio.isPlaying)
                pedalingAudio.Play();
            idleAudio.Stop();
        }
        else {
            if (!idleAudio.isPlaying)
                idleAudio.Play();
            pedalingAudio.Stop();
        }

        float vel = rigidBody.velocity.magnitude;
        pedalingAudio.pitch = vel * 0.15f;
        idleAudio.pitch = vel * 0.15f;
        idleAudio.volume = Mathf.Clamp(vel * 0.5f, 0, 1);
    }
}