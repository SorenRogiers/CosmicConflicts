using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* UIPanel
 * inherits from MonoBehaviour
 * abstract 
 * defines method for panels (initialise, refresh, ...)
 */
public abstract class UIPanel : MonoBehaviour
{
    protected bool _isPanelActive { get; set; }
    protected bool _isPaused { get; set; }

    public abstract void Initialise();
    public abstract void Refresh();

    private void Start()
    {
        gameObject.SetActive(_isPanelActive);
    }

    public void Toggle()
    {
        Refresh();

        _isPanelActive = !_isPanelActive;
        this.gameObject.SetActive(_isPanelActive);

        if (_isPaused)
            Time.timeScale = 0.0f;
        else
            Time.timeScale = 1.0f;
    }

    public void Disable()
    {
        if (this)
            gameObject.SetActive(false);
    }

}
