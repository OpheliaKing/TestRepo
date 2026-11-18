using UnityEngine;

public class MovePanel : MonoBehaviour
{
    [SerializeField] private int _columns = 5;
    [SerializeField] private float _spacingX = 1f;
    [SerializeField] private float _spacingZ = 1.4f;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private RuntimeAnimatorController _moveController;

    private const string MoveParameter = "Move";

    public float MoveSpeed => _moveSpeed;

    private Vector3 _destination;
    private bool _isMoving;
    private bool _layoutLocked;

    private void OnEnable()
    {
        Layout();
    }

    private void OnValidate()
    {
        _columns = Mathf.Max(1, _columns);
        _spacingX = Mathf.Max(0f, _spacingX);
        _spacingZ = Mathf.Max(0f, _spacingZ);
        _moveSpeed = Mathf.Max(0f, _moveSpeed);
        Layout();
    }

    private void OnTransformChildrenChanged()
    {
        if (_layoutLocked)
            return;

        Layout();
    }

    public void Detach(Transform unit)
    {
        if (unit == null || unit.parent != transform)
            return;

        _layoutLocked = true;
        unit.SetParent(null, true);
        _layoutLocked = false;
    }

    public void AddUnit(Transform unit)
    {
        if (unit == null)
            return;

        if (unit.parent == transform)
        {
            Layout();
            return;
        }

        unit.SetParent(transform, false);
    }

    public void MoveTo(Vector3 destination)
    {
        _destination = destination;
        _isMoving = true;
        PlayMoveAnimation(true);
    }

    private void Update()
    {
        if (!_isMoving)
            return;

        transform.position = Vector3.MoveTowards(transform.position, _destination, _moveSpeed * Time.deltaTime);
        if (transform.position == _destination)
        {
            _isMoving = false;
            PlayMoveAnimation(false);
        }
    }

    private void PlayMoveAnimation(bool moving)
    {
        var animators = GetComponentsInChildren<Animator>();
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (_moveController != null)
                animator.runtimeAnimatorController = _moveController;

            animator.applyRootMotion = false;
            animator.SetBool(MoveParameter, moving);
        }
    }

    private void Layout()
    {
        int columns = Mathf.Max(1, _columns);
        int count = transform.childCount;
        float originX = (columns - 1) * _spacingX * 0.5f;

        for (int i = 0; i < count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            Transform child = transform.GetChild(i);
            child.localPosition = new Vector3(column * _spacingX - originX, 0f, row * _spacingZ);
            child.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }
}
