using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class ClickManager : MonoBehaviour
{
    //this class handles the presses of the screen from the user and allows them to click the numbers on the screen
    PlayerInputActions inputManager; //the input actions that store what inputs the player can make
    public GameObject lastSelectedGameObject = null; //stores the last clicked object. Is null if no object is actually selected
    // Start is called before the first frame update
    void Awake()
    {
        inputManager = new PlayerInputActions(); //create new instance
        inputManager.Enable();
        //set a event for when the screen is clicked
        inputManager.Player.Press.performed += ClickedScreen;
    }

    /*
     * Function that handles the event for when the screen is pressed
    */
    void ClickedScreen(InputAction.CallbackContext context)
    {
        Vector2 touchPosition = Touchscreen.current.position.ReadValue(); //get the current position of the touch on the touch screen
        lastSelectedGameObject = GetClickedObject(touchPosition);//check if there is a number there, and assign it to lastSelectedGameObject
    }

    /*
     * Function that checks if there is a number at the position pressed
     * Does this by sending a raycast to the point and past it, and checking what it returns
    */
    GameObject GetClickedObject(Vector2 touchPosition)
    {
        //send raycast from camera to that position 
        Ray ray = Camera.main.ScreenPointToRay((Vector3)touchPosition);
        RaycastHit hit; 
        Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Numbers")); //send raycast
        if(hit.collider!=null)
        {
            return hit.collider.gameObject; //return game object
        }
        return null;
    }


}
