using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* UIManager
 * *********
 * Create the necessary UI Panels
 */
public class UIManager
{ 
    public EndGamePanel EndGamePanel { get; private set; }
    public PhaseInfoPanel PhaseInfoPanel { get; private set; }
    public ObjectivePanel ObjectivePanel { get; private set; }
    public DetailsPanel DetailsPanel { get; private set; }
    public CombatLogPanel CombatLogPanel { get; private set; }
    public TimerPanel TimerPanel { get; private set; }
    public RoundSettingsPanel RoundSettingsPanel { get; private set; }
    public PlayerJoinPanel PlayerJoinPanel { get; private set; }
    public PausePanel PausePanel { get; private set; }

    private Canvas _canvas;

    public UIManager()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        _canvas = GameObject.Find("Game_UI").GetComponent<Canvas>();

        if (currentScene == "MainScene" || currentScene == "ColinTestScene")
        {
            PhaseInfoPanel = CreatePanel("PhaseInfoPanel", new Vector3(800,-200,0)) as PhaseInfoPanel;
            EndGamePanel = CreatePanel("EndGamePanel", Vector3.zero) as EndGamePanel;
            ObjectivePanel = CreatePanel("ObjectivePanel", new Vector3(-750, 450, 0)) as ObjectivePanel;
            DetailsPanel = CreatePanel("DetailsPanel", new Vector3(-800,240,0)) as DetailsPanel;
            CombatLogPanel = CreatePanel("CombatLogPanel", new Vector3(735, -390)) as CombatLogPanel;
            TimerPanel = CreatePanel("TimerPanel", Vector3.zero) as TimerPanel;
            PausePanel = CreatePanel("PausePanel", Vector3.zero) as PausePanel;
        }

        if(currentScene == "PlayerLobby_Scene")
        {
            RoundSettingsPanel = CreatePanel("RoundSettingsPanel", new Vector3(-700, -75, 0)) as RoundSettingsPanel;
            PlayerJoinPanel = CreatePanel("PlayerJoinPanel", new Vector3(700, 0, 0)) as PlayerJoinPanel;
        }

    }
   
    private UIPanel CreatePanel(string prefab, Vector3 position)
    {
        var panelPrefab = Resources.Load<UIPanel>("Prefabs/UI/" + prefab);
        var panel = Object.Instantiate(panelPrefab);
        panel.transform.SetParent(_canvas.transform);
        panel.transform.localPosition = position;
        panel.Initialise();

        return panel;
    }
}
