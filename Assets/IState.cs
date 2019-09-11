using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}


public class StartState : IState
{
    public void Enter()
    {
        return;
    }

    public void Execute()
    {
        return;
    }

    public void Exit()
    {
        return;
    }
}

public class PauseState : IState
{
    private Defragger _defragger;
    private IState _previous;

    public PauseState(Defragger _defragger, IState _previous)
    {
        this._defragger = _defragger;
        this._previous = _previous;

        Enter();
    }

    public void Enter()
    {
        if (_previous != null) Debug.LogFormat("{0} -> IdleState", _previous.GetType());
        Time.timeScale = 0;
    }

    public void Execute()
    {
        return;
    }

    public void Exit()
    {
        Time.timeScale = 1;

        if (_previous != null)
        {
            _defragger.State = _previous;
            _defragger.State.Enter();
        }
    }
}

public class DefaultPlayState : IState
{
    private Defragger _defragger;
    private IState _previous;

    public DefaultPlayState(Defragger _defragger, IState _previous)
    {
        this._defragger = _defragger;
        this._previous = _previous;

        Enter();
    }

    public void Enter()
    {
        if (_previous != null) Debug.LogFormat("{0} -> DefaultPlayState", _previous.GetType());
    }

    public void Execute()
    {
        _defragger.AdvanceTime();
    }

    public void Exit()
    {
        return;
    }
}

public class AutoDefragState : IState
{
    private Defragger _defragger;
    private IState _previous; // DEBUG

    public AutoDefragState(Defragger _defragger, IState _previous)
    {
        this._defragger = _defragger;
        this._previous = _previous;

        Enter();
    }

    public void Enter()
    {
        if (_previous != null) Debug.LogFormat("{0} -> AutoDefragState", _previous.GetType());

        // Start the HDD seeking sounds
        AudioController.instance.StartLooping();

        _defragger.AutoDefraggingLabelText.text = "AUTODEFRAG ENABLED";

        Execute();
    }

    // Defrag one sector at the given AutoDefragRate
    public void Execute()
    {
        _defragger.TimeLeft -= Time.deltaTime;
        _defragger.AdvanceTime();
        if (_defragger.TimeLeft <= 0)
        {
            _defragger.DefragOne();
            _defragger.TimeLeft = ((float)_defragger.AutoDefragRate / 10f);
        }

        if (_defragger.TotalSectorsToDefrag == _defragger.SectorsDefragged)
        {
            _defragger.State = new CompleteState(_defragger, this);
        }
    }

    public void Exit()
    {       
        // Stop the HDD seeking sounds
        AudioController.instance.EndLooping();

        _defragger.AutoDefraggingLabelText.text = "AUTODEFRAG DISABLED";
        _defragger.State = new DefaultPlayState(_defragger, this);
    }
}

public class FreePaintingState : IState
{
    private Defragger _defragger;

    public FreePaintingState(Defragger _defragger)
    {
        this._defragger = _defragger;
    }

    public void Enter()
    {
        Sector[] sectorChildren = _defragger.SectorsPanel.GetComponentsInChildren<Sector>();

        foreach (Sector sector in sectorChildren)
        {
            if (sector.State == Constants.SECTOR_DEFRAGMENTED)
            {
                sector.State = Constants.SECTOR_FRAGMENTED;
                sector.Glyph.color = Constants.ColorFragmented;
            }

            sector.gameObject.tag = "UIDraggable";
        }

        if (_defragger.IsDefragComplete)
        {
            _defragger.IsDefragComplete = false;
        }

        _defragger.StartCheckingFromIndex = 0;
        _defragger.SectorsDefragged = 0;
        _defragger.ProgressBarBlocksToFill = 0;
        _defragger.Percentage = 0;

        CompletionBar.Instance.ResetBar();
        _defragger.CompletionText.text = string.Format("Completion                  0%");

        _defragger.AutoDefraggingLabelText.text = "AUTODEFRAG DISABLED";
    }

    public void Execute()
    {
        return;
    }

    public void Exit()
    {
        // Get all the sectors that have be defragged
        _defragger.TotalSectorsToDefrag = _defragger.GetFragmentedSectors().Count;

        // Scan the grid for sectors already in position to be defragged
        _defragger.ScanGrid();

        CompletionBar.Instance.FillProgressBar(_defragger.ProgressBarBlocksToFill);
    }
}

public class CompleteState : IState
{
    IState _previous;
    private Defragger _defragger;

    public CompleteState(Defragger _defragger, IState _previous)
    {
        this._defragger = _defragger;
        this._previous = _previous;
        //Enter();
    }

    public void Enter()
    {
        if (_previous != null) Debug.LogFormat("{0} -> CompleteState", _previous.GetType());

        foreach (Sector sector in _defragger.AllSectors)
        {
            sector.gameObject.tag = "Untagged";
        }

        _defragger.FooterText.text = "Finished condensing";

        if (_previous.GetType() == typeof(AutoDefragState))
        {
            if (_defragger.IsAutoDefragEndless)
            {
                _defragger.NewHDD();
            }
            else
            {
                AudioController.instance.EndLooping();
            }
        }
        else
        {
            AudioController.instance.EndLooping();
        }

        Exit();
    }

    public void Execute()
    {
        return;
    }

    public void Exit()
    {
        if (_previous.GetType() == typeof(AutoDefragState))
        {
            _defragger.State = new AutoDefragState(_defragger, this);
        }
    }
}