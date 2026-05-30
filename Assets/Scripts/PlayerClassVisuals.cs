using UnityEngine;

[RequireComponent(typeof(InventoryManager))]
public class PlayerClassVisuals : MonoBehaviour
{
    [Header("Animators")]
    public RuntimeAnimatorController warriorController;
    public RuntimeAnimatorController archerController;
    public RuntimeAnimatorController lancerController;

    private InventoryManager inventoryManager;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        inventoryManager = GetComponent<InventoryManager>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        switch (inventoryManager.playerClass)
        {
            case PlayerClass.Warrior:
                animator.runtimeAnimatorController = warriorController;
                break;
            case PlayerClass.Archer:
                animator.runtimeAnimatorController = archerController;
                break;
            case PlayerClass.Lancer:
                animator.runtimeAnimatorController = lancerController;
                break;
        }
    }
}
