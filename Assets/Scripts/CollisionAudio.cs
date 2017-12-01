using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CollisionAudio : MonoBehaviour {
	private AudioSource audio;
	
	// Use this for initialization
	void Start () {
		audio = GetComponent<AudioSource>();
	}

	private void OnCollisionEnter(Collision other) {
		if (other.gameObject.tag == "Player" && !audio.isPlaying) {
			float force = other.impulse.magnitude;
			audio.volume = Mathf.Clamp(force / 1000.0f, 0, 1);
			audio.Play();
		}
	}
}
