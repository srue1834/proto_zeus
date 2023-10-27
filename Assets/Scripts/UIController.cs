using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI wakePrompt;
    public TextMeshProUGUI callZeusPrompt;
    public TextMeshProUGUI runPrompt;
    public TextMeshProUGUI carryPrompt;
    public TextMeshProUGUI pickUpBallPrompt;
    public TextMeshProUGUI throwBallPrompt;

    // Use these methods to show/hide prompts
    public void ShowWakePrompt(bool show) { ShowPrompt(wakePrompt, show); }
    public void ShowCallZeusPrompt(bool show) { ShowPrompt(callZeusPrompt, show); }
    public void ShowRunPrompt(bool show) { ShowPrompt(runPrompt, show); }
    public void ShowCarryZeusPrompt(bool show) { ShowPrompt(carryPrompt, show); }
    public void ShowPickUpBallPrompt(bool show) { ShowPrompt(pickUpBallPrompt, show); }
    public void ShowThrowBallPrompt(bool show) { ShowPrompt(throwBallPrompt, show); }

    private void ShowPrompt(TextMeshProUGUI prompt, bool show)
    {
        prompt.transform.parent.gameObject.SetActive(show);
    }
}
