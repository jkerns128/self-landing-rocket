using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketHandler : MonoBehaviour
{
    public int rocketThrust = 445000;
    public int fuel = 1000;
    Rigidbody rigidbody;
    Transform transform;
    Vector3 velocity;
    Vector3 rocketRotation;
    double thrustThreshold;
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
        rocketRotation = transform.up;
        /*
            Rocket aerodynamics physics
        */
        Vector3 rocketLine = rocketRotation;
        Vector3 cross = Vector3.Cross(velocity, rocketLine);   
        float angle = Vector3.SignedAngle(velocity, rocketLine, cross);
        Vector3 dragForce;
        
        //Flips "up" direction for rocket to simplify calculation of drag
        if (Mathf.Abs(angle) > 90){
            rocketLine *= -1;
            angle = Vector3.SignedAngle(velocity, rocketLine, cross);
        }
        dragForce = Quaternion.AngleAxis(Mathf.Sign(angle)*(90-Mathf.Abs(angle)), rocketLine) * velocity;
        dragForce = dragForce.normalized * Mathf.Sqrt(velocity.magnitude);

        rigidbody.AddRelativeForce(dragForce);


        /*
            Uses turning to glide over the landing space, simple math for negating velocity on landing pad.
        */
        
        


        /*  Landing Control */
        thrustThreshold = Mathf.Pow(velocity.y,2) / (2* (rocketThrust/rigidbody.mass));
        if(transform.position.y - 7.5 < thrustThreshold && fuel > 0){
            fuel = fuel - 1;
            rigidbody.AddRelativeForce(rocketRotation*rocketThrust);
        //Particle control from here down (visual only)
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

    // Destroy Rocket on high impact speed
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 3)
            Destroy(gameObject);
    }

}
