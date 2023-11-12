using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Number : MonoBehaviour
{
    //This class stores information and functions for each number spawned
    private AudioSource audioSource; //the audio clip for this number
    private Rigidbody rigid; //the rigid body on this number
    private MeshRenderer meshRenderer; //renderer on the prefab
    private bool shouldBounce = false; //decides whether the number should bounce around or just stay still
    private Vector3 previousVelocity; //used to store the previous velocity and prevent the number from getting stuck
    [SerializeField] private GameObject effect; //the particle effect played when the number is clicked correctly
    public int number; //the number this object represents
    // Start is called before the first frame update
    void Awake()
    {
        //get components
        audioSource = GetComponent<AudioSource>();
        rigid = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldBounce)
        {
            //make the number bounce around if it is meant to
            BounceAround();
        }
    }

    /*
     * Plays the audio clip for this number (that reads out the number) 
    */
    public void ReadOut()
    {
        audioSource.time = 0;
        audioSource.Play();
    }

    /*
     * Hides the gameobject so it doesn't appear in the scene
     * This is necessary because we don't want to destroy the number straight away (e.g. when the audio is still playing)
    */ 
    public void Hide()
    {
        if(meshRenderer!=null)
        {
            meshRenderer.enabled = false;
        }
        else
        {
            //this case is for the number 10, which consists of two number objects. Here, it goes over both and disables the renderer
            foreach(MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
            {
                mesh.enabled = false;
            }
        }
    }

    /*
     * Shows the particle effect. We instantiate it instead of play it as we want the effect to persist after this is destroyed
     * The particle system destroys itself after it finishes playing
    */ 
    public void PlayEffect()
    {
        Instantiate(effect, transform.position, Quaternion.identity, transform.parent); 
    }

    /*
     * This function is run to allow the number to bounce around
     * It also determines an initial velocity that is randomly set
    */ 
    public void ShouldBounce()
    {
        shouldBounce = true;
        int randomInt = Random.Range(0, 2) == 0 ? 1 : -1;
        rigid.velocity = new Vector3(1.5f * randomInt, 1.5f * randomInt);

    }

    /*
     * This function makes the number bounce around
     * It sets their rigid velocity to a set amount to ensure that it keeps moving at the same speed
     * It also handles when the number stops and makes sure it keeps moving 
    */
    private void BounceAround()
    {
        if(rigid.velocity == Vector3.zero)
        {
            rigid.velocity = new Vector3(Mathf.Sign(previousVelocity.x) * -1, Mathf.Sign(previousVelocity.y) * -1); //make the number move in the opposite direction so it doesn't go through a wall
        }
        else
        {
            previousVelocity = rigid.velocity; //assign previous velocity when the rigid velocity is not 0
        }
        rigid.velocity = rigid.velocity.normalized * 1.5f; //move constantly 
    }

    /*
     * Handles when the number collides with a bouncy wall
     * Although the physics material is enough to allow the number to bounce off, this function tries to make the direction more random by adding a small vector to the rigid velocity
     * This means that the numbers won't be bouncing just in the same directions (e.g. left to right to left etc.)
    */
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag=="Wall")
        {
            //change direction slightly
            rigid.velocity = - rigid.velocity;
            int randomInt = Random.Range(0, 2) == 0 ? 1 : -1;
            //below we make sure that adding the vector doesn't make us go past the wall we just hit
            //we do this by checking the sign of the new x and y component and making sure that we are still moving in the same direction as we were before (away from the wall)
            if(Mathf.Sign(rigid.velocity.x - randomInt) == Mathf.Sign(rigid.velocity.x) && rigid.velocity.x - randomInt!=0)
            {
                rigid.velocity -= new Vector3(randomInt, 0, 0);
            }
            if (Mathf.Sign(rigid.velocity.y - randomInt) == Mathf.Sign(rigid.velocity.y) && rigid.velocity.y - randomInt != 0)
            {
                rigid.velocity -= new Vector3(0, randomInt, 0);
            }
        }
    }
}
