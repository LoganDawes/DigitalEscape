using UnityEngine;

/*
 
    TimedButton : Platforming
    Button that only stays active for a set amount of time when pressed.
 
 */

public class TimedButton : Button
{
    // Variables
    [Header("Timed Button")]
    [SerializeField] private float timer = 2f;
    private float timerCounter = 0f;
    private bool timing = false;

    // Update
    protected override void Update()
    {
        // Base update for player interaction
        base.Update();

        if (timing)
        {
            timerCounter -= Time.deltaTime;
            if (timerCounter <= 0f)
            {
                timing = false;
                OnPressed(); // Un-activate
            }
        }
    }

    // OnPressed Override
    public override void OnPressed()
    {
        if (!isActive)
        {
            base.OnPressed();
            timing = true;
            timerCounter = timer;
        }
        else
        {
            base.OnPressed();
            timing = false;
        }
    }
}