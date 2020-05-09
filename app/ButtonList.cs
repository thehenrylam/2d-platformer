using Godot;
using System;
using System.Collections.Generic;

public class ButtonList : ColorRect
{
    [Export]
    public int SelectedIndex
    {
        get { return this.selectedIndex; }
        set 
        {
            Deselect(this.selectedIndex);
            this.selectedIndex = value;
            Select(this.selectedIndex);
        }
    }

    public Button CurrentButton
    {
        get { return GetButtonByIndex(this.selectedIndex); }
    }

    public int Count
    {
        get { return this.nodeNames.Count; }
    }

    public List<string> NodeNames
    {
        get
        {
            List<string> output = new List<string>(this.nodeNames);
            return output;      // output the list of node names
        }
    }

    private List<string> nodeNames = new List<string>();
    private int selectedIndex = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.nodeNames = GetNodeNames();

        if (selectedIndex >= 0)
        {
            Select(selectedIndex);
        }
    }

    private List<string> GetNodeNames()
    {
        List<string> output = new List<string>();

        foreach (Node n in this.GetChildren())
        {
            Button b = null;

            try
            {
                b = (Button)n;
            }
            catch (InvalidCastException)
            {
                GD.Print(String.Format("[WARNING] [ButtonList] Unable to cast Node '{0}' into Button type", n.Name));
                continue;
            }

            if (b != null)
            {
                output.Add(b.Name);
            }
        }

        return output;
    }

    private bool Select(int index)
    {
        return SetStateToButton(index, true);
    }

    private bool Deselect(int index)
    {
        return SetStateToButton(index, false);
    }
    
    private Button GetButtonByIndex(int index)
    {
        string nodeName = this.nodeNames[index];
        return this.GetNode<Button>(nodeName);
    }

    private bool SetStateToButton(int index, bool enabled)
    {
        if ((index < 0) || (index >= this.nodeNames.Count)) { return false; }

        Button button = GetButtonByIndex(index);
        button.Enabled = enabled;

        return true;
    }

}
