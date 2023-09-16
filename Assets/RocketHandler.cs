using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketHandler : MonoBehaviour
{
    public int rocketThrust = 445000;
    public int fuel = 100;
    Rigidbody rigidbody;
    Transform transform;
    Vector3 velocity;
    Quaternion rotation;
    double distance;
    GameObject particleSys;
    int usagetimer;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        transform = GetComponent<Transform>();
        particleSys = transform.Find("Particle System").gameObject;

        

    }

    // Update is called once per frame
    void Update()
    {
        velocity = rigidbody.velocity;
        rotation = rigidbody.rotation;
        /*
            Can move proportional to its velocity to simulate using aerodynamics

            PID control to get it over landing space, simple math for negating velocity on landing pad.
        */
        
        
        distance = Mathf.Pow(velocity.y,2) / (2* (rocketThrust/rigidbody.mass));
        if(transform.position.y - 7.5 < distance && fuel > 0){
            fuel = fuel - 1;
            rigidbody.AddRelativeForce(new Vector3(0,rocketThrust,0));
            usagetimer = 100;
            particleSys.SetActive(true);
        } else {
            if(usagetimer < 1){
                particleSys.SetActive(false);
            } else {
                usagetimer = usagetimer - 1;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
        // Destroy Rocket on high impact speed
        if (collision.relativeVelocity.magnitude > 15)
            Destroy(gameObject);
    }

}
