using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    //This class handles the logic of the game
    public static bool gameStart = false; //a static bool that determines if the game has started
    private bool numberSelected = false; //tells us if we have selected the number to read out and test
    private bool waitingForNumberToBeTapped = false; //tells us if we are waiting for the user to tap a number (when the numbers are bouncing around)
    private List<int> numbersLeft = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; //a list of numbers 1-10 that is removed from whenever we select a number to be tested
    private int numbersCorrectlyChosen = 0; //stores how many numbers the player has correctly chosen on their first try. Their score
    private bool failedCurrentNumberOnce = false;//tells us if the player has previously failed the number selection. If true, we don't add to the score
    private int numberToBeTested; //stores the number currently selected to be tested
    private bool inCoroutine = false; //tells us if we are in a coroutine (as to not start another one)
    //A long list of references that are needed
    [Header("UI Components")]
    [SerializeField] GameObject StartUI;
    [SerializeField] GameObject EndGameUI;
    [SerializeField] TMPro.TextMeshProUGUI score;
    [SerializeField] TMPro.TextMeshProUGUI textPrompt;
    [SerializeField] EventSystem eventSystem;
    [Header("Other References")]
    [SerializeField] Number_Spawner spawner;
    [SerializeField] ClickManager clickManager;
    [SerializeField] ParticleSystem endGameEffects;
    [SerializeField] ParticleSystem endGameEffects2;
    [Header("Sound Clips")]
    [SerializeField] AudioSource congratulationsClip;
    [SerializeField] AudioSource correctAnswerClip;
    [SerializeField] AudioSource incorrectAnswerClip;

    /*
     * Function to choose what number to spawn next and test
    */
    void SelectNumber()
    {
        //randomly choose number out of listOfNumbers
        int numberChosen = numbersLeft[Random.Range(0, numbersLeft.Count)];
        //remove the number from the list
        numbersLeft.Remove(numberChosen);
        spawner.Spawn(numberChosen); //spawn it
        numberToBeTested = numberChosen;
        numberSelected = true;
    }


    /*
     * Function that decides what numbers to spawn to bounce around
     * The numbers are randomly selected, with the number to be tested included as well
    */
    void GiveNumberChoices()
    {
        int amountOfNumbers = Random.Range(3, 5); //decide how many numbers to spawn
        int randomIndex = Random.Range(0, amountOfNumbers);
        List<int> numbersToSpawn = new List<int>(); //list which is passed into the number spawner. Stores the numbers we want to spawn as objects
        waitingForNumberToBeTapped = true; //shows we are now waiting for the user to select the correct number
        List<int> allNumbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        allNumbers.Remove(numberToBeTested);
        for (int i=0; i< amountOfNumbers; i++)
        {
            if(i==randomIndex)
            {
                numbersToSpawn.Add(numberToBeTested); //add the number to be tested at a random index
            }
            else
            {
                //choose random number that is not this number
                int randomNumber = allNumbers[Random.Range(0, allNumbers.Count)];
                allNumbers.Remove(randomNumber);
                numbersToSpawn.Add(randomNumber);
            }
        }
        spawner.SpawnListOfNumbers(numbersToSpawn); //Spawn all the numbers
    }

    /*
     * Check if the number that was clicked is the correct number 
    */
    void CheckNumberClicked(int number)
    {
        //If the number is the correct one
        if(number==numberToBeTested)
        {
            //Congratulate User, and move onto the next number
            StartCoroutine(CongratulateUser());
            spawner.DestroyAllNumbers();
            waitingForNumberToBeTapped = false;
            if(!failedCurrentNumberOnce)
            {
                numbersCorrectlyChosen += 1;
            }
            failedCurrentNumberOnce = false;
            //If no more numbers left, end the game, otherwise, get the next number
            if (numbersLeft.Count==0)
            {
                //end game
                End_Game();
            }
            else
            {
                clickManager.lastSelectedGameObject.GetComponent<Number>().PlayEffect(); //show effect
                numberSelected = false;
            }
        }
        else
        {
            StartCoroutine(IncorrectAnswer()); //if the number was incorrect, try again
        }
    }

    /*
     * Function that ends the game. Shows some effects and the score, and allows the player to press play again 
    */
    void End_Game()
    {
        endGameEffects.time = 0;
        endGameEffects.Play();
        endGameEffects2.time = 0;
        endGameEffects2.Play();
        textPrompt.enabled = false;
        score.text = "You got " + numbersCorrectlyChosen + "/10!";
        EndGameUI.SetActive(true);
    }

    /*
     * Function run from the UI when the play again button is pressed
     * Resets all the variables and allows the game to be played again
    */
    public void PlayAgain()
    {
        eventSystem.SetSelectedGameObject(null); //deselect the play again button otherwise it clicks twice next time
        spawner.DestroyAllNumbers();
        EndGameUI.SetActive(false);
        //reset all variables
        numberSelected = false;
        waitingForNumberToBeTapped = false;
        numbersLeft = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        numbersCorrectlyChosen = 0;
        failedCurrentNumberOnce = false;
        numberToBeTested = 0;
        inCoroutine = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameStart) //if the game has started
        {
            //if we haven't selected the next number, then select it
            if(!numberSelected && !inCoroutine)
            {
                StartCoroutine(SelectNextNumber());
            }
            else
            {
                //otherwise, we have selected it
                //if they have tapped the number already and we haven't spawned the selection of numbers, then spawn it
                if(clickManager.lastSelectedGameObject!=null && !waitingForNumberToBeTapped && !inCoroutine)
                {
                    //read out the number selected, and get ready to spawn the selection of numbers
                    clickManager.lastSelectedGameObject.GetComponent<Number>().ReadOut();
                    StartCoroutine(WaitToShowSelection());
                }
                else if(waitingForNumberToBeTapped)
                {
                    //here, the numbers have been spawned
                    //if a number has been clicked, check if it is correct
                    if(clickManager.lastSelectedGameObject != null)
                    {
                        //check the number
                        CheckNumberClicked(clickManager.lastSelectedGameObject.GetComponentInChildren<Number>().number);
                    }
                }
            }
        }
    }

    //Below are the Coroutines, which primarily control the showing of the text

    /*
     * Coroutine that gets ready to spawn the selection of numbers
     * Before that, it shows some next to prompt the player to repeat the number out loud and prepare them to tap the correct number
    */
    IEnumerator WaitToShowSelection()
    {
        inCoroutine = true;
        clickManager.lastSelectedGameObject.GetComponent<Number>().Hide();//to hide the number
        //show text prompt
        textPrompt.enabled = true;
        textPrompt.text = "Try to repeat this number out loud";
        yield return new WaitForSeconds(4);
        textPrompt.text = "Great Work!";
        yield return new WaitForSeconds(2);
        textPrompt.text = "Pick the number you just saw from the incoming options";
        yield return new WaitForSeconds(3);
        StartCoroutine(ShowCountdown()); //3, 2, 1 countdown
        yield return new WaitForSeconds(3);
        //after countdown, spawn the numbers
        spawner.DestroyAllNumbers();
        clickManager.lastSelectedGameObject = null;
        GiveNumberChoices();
        inCoroutine = false;
    }

    /*
     * Coroutine that shows the countdown 3, 2, 1 in text 
    */
    IEnumerator ShowCountdown()
    {
        textPrompt.enabled = true;
        textPrompt.fontSize += 20; //increase font size to make it more visible
        textPrompt.text = "3";
        yield return new WaitForSeconds(1);
        textPrompt.text = "2";
        yield return new WaitForSeconds(1);
        textPrompt.text = "1";
        yield return new WaitForSeconds(1);
        textPrompt.fontSize -= 20;
        textPrompt.enabled = false;
    }

    /*
     * Prepare the user to select the next number after some text
     * We then spawn the next number for them to press
    */
    IEnumerator SelectNextNumber()
    {
        inCoroutine = true;
        textPrompt.enabled = true;
        textPrompt.text = "Tap the number that has appeared. You may have to look around for it";
        yield return new WaitForSeconds(3);
        textPrompt.enabled = false;
        SelectNumber();
        inCoroutine = false;
    }

    /*
     * Play congratulations clip and show text 
    */
    IEnumerator CongratulateUser()
    {
        inCoroutine = true;
        congratulationsClip.time = 1;
        congratulationsClip.Play(); //well done
        correctAnswerClip.time = 0;
        correctAnswerClip.Play();
        textPrompt.enabled = true;
        textPrompt.text = "Well Done!";
        yield return new WaitForSeconds(3);
        textPrompt.enabled = false;
        inCoroutine = false;
    }

    /*
     * Play incorrect answer clip and get ready to show the selection again (so that they can try again to press the number)
    */
    IEnumerator IncorrectAnswer()
    {
        inCoroutine = true;
        textPrompt.enabled = true;
        spawner.DestroyAllNumbers();
        incorrectAnswerClip.time = 0;
        incorrectAnswerClip.Play();
        textPrompt.text = "Oops! Please try again";
        yield return new WaitForSeconds(3);
        StartCoroutine(ShowCountdown());
        yield return new WaitForSeconds(3);
        clickManager.lastSelectedGameObject = null;
        GiveNumberChoices(); //show number choices again
        inCoroutine = false;
    }
}
