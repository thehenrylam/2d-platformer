using System;
using System.Collections.Generic;

public class PlayerState
{
    public event EventHandler PlayerDied;

    public bool CanJump 
    {
        get 
        {
            // Can only jump when...
            // the player is (touching the ground OR coyoteTime is active) AND
            // did not jump before hand.
            return ((touchingGround || (coyoteTimeState == 1)) && (jumpTally <= 0)); 
        }
    }

    public bool CanFastFall
    {
        get 
        { 
            // Can only fast fall when...
            // the player did not fast fall before.
            return (fastFallTally == 0); 
        }
    }

    public bool CanAirDash
    {
        get 
        {
            // Can only air dash when...
            // the player is NOT touching the ground AND
            // did not air dash beforehand AND
            // an air dash is not currently active.
            return (((!touchingGround) && (airDashTally <= 0)) && (this.airDashState == 0));
        }
    }

    public bool IsDead
    {
        get { return deathFlag; }
        set
        {
            deathFlag = value;
            if (deathFlag)
            {
                PlayerDied?.Invoke(this, null);
            }
        }
    }

    public bool IsGrounded
    {
        get { return this.touchingGround; }
        set
        {
            // Evaluate further if there is a change in value for the touchingGround flag.
            if (value != this.touchingGround)
            {
                this.touchingGround = value;
                if (this.touchingGround)
                {
                    // If it the state recently landed,
                    // then reset all tallies and the coyoteTimeState.
                    this.coyoteTimeState = 0;
                    this.jumpTally = 0;
                    this.fastFallTally = 0;
                    this.airDashTally = 0;
                }
                else 
                {
                    if ((this.jumpTally == 0) && (this.coyoteTimeState == 0))
                    {
                        // If the player has not jumped before 
                        // and the coyoteTimeState is primed,
                        // then activate coyote time.
                        this.coyoteTimeState = 1;
                    }
                }
            }
        }
    }

    public bool IsAirDashActive
    {
        get { return (this.airDashState == 1); }
    }

    public bool IsDashing
    {
        get { return false; }
    }

    private bool touchingGround = true;
    private int jumpTally = 0;
    private int airDashTally = 0;
    private int fastFallTally = 0;
    private int coyoteTimeState = 0;
    private int airDashState = 0;
    private bool deathFlag = false;
    
    public PlayerState()
    {

    }

    public bool Jumped(bool force=false)
    {
        bool isLegal = this.CanJump;

        if (isLegal || force)
        {
            this.jumpTally += 1;
            this.fastFallTally = 0;
        }

        return isLegal;
    }

    public bool AirDashed(bool force=false)
    {
        bool isLegal = this.CanAirDash;

        if (isLegal || force)
        {
            this.airDashTally += 1;
            this.airDashState = 1;
        }

        return isLegal;
    }

    public bool AirDashFinished(bool force=false)
    {
        bool isLegal = (this.airDashState == 1);

        if (isLegal || force)
        {
            this.airDashState = 0;
        }

        return isLegal;
    }

    public bool FastFallen(bool force=false)
    {
        bool isLegal = this.CanFastFall;

        if (isLegal || force)
        {
            this.fastFallTally += 1;
        }

        return isLegal;
    }

    public bool Damaged(int dmg, bool force=false)
    {
        bool isLegal = !this.IsDead;

        if (isLegal || force)
        {
            this.IsDead = true;
        }

        return isLegal;
    }

    public bool CoyoteTimeFinished(bool force=false) 
    {
        this.coyoteTimeState = -1;

        return true;
    }

    

}
