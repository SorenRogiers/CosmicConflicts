using UnityEngine.UI;

public class PhaseInfoPanel : UIPanel
{
    private Text _phaseNumber;
    private Text _phaseName;
    private bool _isShown = false;
    public override void Initialise()
    {
        _isPanelActive = true;

        _phaseName = gameObject.transform.Find("PhaseName").GetComponent<Text>();
        _phaseNumber = gameObject.transform.Find("PhaseNumber").GetComponent<Text>();
    }

    public override void Refresh()
    {
        int phaseNumber = RoundSystem.Instance()._phase;
        string phaseName = "";
        
        switch(phaseNumber)
        {
            case 0:
                phaseName = "Intro";
                break;
            case 1:
                phaseName = "Move";
                break;
            case 2:
                phaseName = "Shoot";
                break;
            case 3:
                phaseName = "Evade";
                break;
            case 4:
                phaseName = "Reward";
                break;
        }

        _phaseNumber.text = phaseNumber.ToString();
        _phaseName.text = phaseName;
    }

    public void Show()
    {
        if (_isShown)
            return;

        _isShown = true;

        this.gameObject.SetActive(true);
    }
    public new void Disable()
    {
        _isShown = false;
        this.gameObject.SetActive(false);
    }
}
