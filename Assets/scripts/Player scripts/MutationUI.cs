using UnityEngine;
using UnityEngine.UI;

public class MutationUI : MonoBehaviour
{
    public PlayerController playerController;

    public Button clawButton;
    public Button armorButton;
    public Button monsterEyeButton;
    public Button atrophiedWingButton;
    public Button tentacleButton;
    public Button bioluminescentAntennaButton;
    public Button removeAllMutationsButton;

    public Mutation clawMutation;
    public Mutation armorMutation;
    public Mutation monsterEyeMutation;
    public Mutation atrophiedWingMutation;
    public Mutation tentacleMutation;
    public Mutation bioluminescentAntennaMutation;

    void Start()
    {
        clawButton.onClick.AddListener(() => OnApplyMutationButton(clawMutation));
        armorButton.onClick.AddListener(() => OnApplyMutationButton(armorMutation));
        monsterEyeButton.onClick.AddListener(() => OnApplyMutationButton(monsterEyeMutation));
        atrophiedWingButton.onClick.AddListener(() => OnApplyMutationButton(atrophiedWingMutation));
        tentacleButton.onClick.AddListener(() => OnApplyMutationButton(tentacleMutation));
        bioluminescentAntennaButton.onClick.AddListener(() => OnApplyMutationButton(bioluminescentAntennaMutation));
        removeAllMutationsButton.onClick.AddListener(OnRemoveAllMutationsButton);
    }

    public void OnApplyMutationButton(Mutation mutation)
    {
        playerController.ApplyMutation(mutation);
    }

    public void OnRemoveAllMutationsButton()
    {
        if (playerController != null)
        {
            playerController.RemoveAllMutations();
        }
        else
        {
            Debug.LogWarning("PlayerController reference is null in MutationUI.");
        }
    }
}
