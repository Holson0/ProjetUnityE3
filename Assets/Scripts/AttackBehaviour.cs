using UnityEngine;

public class AttackBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Equipment equipmentSystem;

    [SerializeField]
    private UIManager uiManager;

    [SerializeField]
    private InteractBehaviour interactBehaviour;

    [Header("Configuration")]
    private bool isAttacking;

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private Vector3 attackOffset;

    void Update()
    {

        if(Input.GetMouseButtonDown(0) && CanAttack())
        {
            isAttacking = true;
            SendAttack();
            animator.SetTrigger("Attack");
        }
    }

    void SendAttack()
    {
        Debug.Log("Attack sent");

        RaycastHit hit;

        if(Physics.Raycast(transform.position + attackOffset, transform.forward, out hit, attackRange, layerMask))
        {
            if(hit.transform.CompareTag("AI"))
            {
                EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
                enemy.TakeDammage(equipmentSystem.equipedWeaponItem.attackPoints);
            }
        }
    }

    bool CanAttack()
    {
        return equipmentSystem.equipedWeaponItem != null && !isAttacking && !uiManager.atLeastOnePanelOpened && !interactBehaviour.isBusy;
    }

    public void AttackFinished()
    {
        isAttacking = false;
    }
}
