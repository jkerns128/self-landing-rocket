using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketHandler : MonoBehaviour
{
    public int rocketThrust = 445000;
    public int fuel = 1000;
    public GameObject landingPad;
    public bool debugLines = true;
    Rigidbody rigidbody;
    Transform transform;
    Vector3 velocity;
    Vector3 rocketRotation;
    double thrustThreshold;
    ParticleSystem particleSys;
    int usagetimer;
    Vector3 aeroForce;
    
    float Xerror_prior = 0;
    float Zerror_prior = 0;
    float Xintegral_prior = 0;
    float Zintegral_prior = 0;
    public float KP = 0.5f; //Some value you need to come up
    public float KI = 0.0f; //Some value you need to come up
    public float KD = 0.0f; //Some value you need to come up
    float bias = 0;

    bool landing = false;

    public float Zderivative;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        transform = GetComponent<Transform>();
        particleSys = transform.Find("Particle System").gameObject.GetComponent<ParticleSystem>();
        



    }
    
    void FixedUpdate()
    {
        velocity = rigidbody.velocity;
        rocketRotation = transform.up;
        
        //Rocket aerodynamics physics
        

        Vector3 proj = Vector3.Dot(rocketRotation, velocity) * rocketRotation;
        aeroForce = proj-velocity;
        aeroForce = aeroForce * Mathf.Pow(velocity.magnitude, 2);

        rigidbody.AddForce(aeroForce);

        if(debugLines){
            Debug.DrawLine(transform.position, transform.position + aeroForce/rigidbody.mass*10, Color.red);
            Debug.DrawLine(transform.position, transform.position + velocity, Color.black);
        }    


        
        /*
            Uses turning to glide over the landing space, simple math for negating velocity on landing pad.
        */
        
        thrustThreshold = Mathf.Pow(velocity.y,2) / (2* (rocketThrust/rigidbody.mass));
        if(landing || transform.position.y - 8.5 < thrustThreshold){
            return;
        }
        
        
        //Birds eye view of rocket and landing pad
        
        Vector3 target = Vector3.ProjectOnPlane(landingPad.GetComponent<Transform>().position, Vector3.up);
        Vector3 position = Vector3.ProjectOnPlane(transform.position, Vector3.up);
        Vector3 moveDirection = target - position;
        
        //PID Controller
        float iteration_time = Time.deltaTime;
        float Xerror = Mathf.Clamp(moveDirection.x, -300, 300);
        float Xintegral = Xintegral_prior;
        if(Xerror < 1 && Xerror > -1){
            Xintegral = Xintegral_prior + Xerror * iteration_time;
        }
        
        float Xderivative = (Xerror - Xerror_prior) / iteration_time;
        float Xoutput = Mathf.Clamp((KP*Xerror/300 + KI*Xintegral + KD*Xderivative + bias), -1, 1);
        Xerror_prior = Xerror;
        Xintegral_prior = Xintegral;

        float Zerror = Mathf.Clamp(moveDirection.z, -300, 300);
        float Zintegral = Xintegral_prior;
        if(Zerror < 1 && Zerror > -1){
            Zintegral = Zintegral_prior + Zerror * iteration_time;
        }
        Zderivative = (Zerror - Zerror_prior) / iteration_time;
        float Zoutput = Mathf.Clamp((KP*Zerror/300 + KI*Zintegral + KD*Zderivative + bias), -1, 1);
        Zerror_prior = Zerror;
        Zintegral_prior = Zintegral;

        //Mapping PID input onto a circle
        
        float newX = Xoutput * Mathf.Sqrt(1 - Zoutput * Zoutput / 2 );
        float newZ = Zoutput * Mathf.Sqrt(1 - Xoutput * Xoutput / 2 );

        transform.up = new Vector3(-newX, 1.1f, -newZ);

        if(debugLines){
            Debug.DrawLine(transform.position, transform.position + (new Vector3(-newX, 1.1f, -newZ))*20, Color.blue);

        }  

    }
    

    // Update is called once per frame
    void Update()
    {
        
        velocity = rigidbody.velocity;
        rocketRotation = transform.up;

        /*  Landing Control (kinematics to determine thrust) */
        thrustThreshold = Mathf.Pow(velocity.y,2) / (2* (rocketThrust/rigidbody.mass));
        var emission = particleSys.emission;
        if(transform.position.y - 8.5 < thrustThreshold && fuel > 0){
            landing = true;
            transform.up = Vector3.up;
            fuel = fuel - 1;
            rigidbody.AddRelativeForce(rocketRotation*rocketThrust);
        //Particle control from here down (visual only)
            emission.enabled = true;
        } else {
            emission.enabled = false;
        } 

    }

    /* Destroy Rocket on high impact speed */
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.y > 5)
            Destroy(gameObject);
    }

}
