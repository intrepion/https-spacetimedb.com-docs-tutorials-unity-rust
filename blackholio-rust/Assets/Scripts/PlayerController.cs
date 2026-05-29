using System.Collections.Generic;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    const int SEND_UPDATES_PER_SEC = 20;
    const float SEND_UPDATES_FREQUENCY = 1f / SEND_UPDATES_PER_SEC;

    public static PlayerController Local { get; private set; }

    private int PlayerId;
    private string username;
    private float LastMovementSendTimestamp;
    private Vector2? LockInputPosition;
    private List<CircleController> OwnedCircles = new List<CircleController>();

    public string Username => username;
    public int NumberOfOwnedCircles => OwnedCircles.Count;
    public bool IsLocalPlayer => this == Local;

    public void Initialize(Player player)
    {
        PlayerId = player.PlayerId;
        username = player.Name;
        if (player.Identity == GameManager.LocalIdentity)
        {
            Local = this;
        }
    }

    public void OnPlayerUpdated(Player player)
    {
        username = player.Name;

        foreach (var circle in OwnedCircles)
        {
            if (circle != null)
            {
                circle.SetName(username);
            }
        }
    }

    private void OnDestroy()
    {
        // If we have any circles, destroy them
        foreach (var circle in OwnedCircles)
        {
            if (circle != null)
            {
                Destroy(circle.gameObject);
            }
        }
        OwnedCircles.Clear();
    }

    public void OnCircleSpawned(CircleController circle)
    {
        OwnedCircles.Add(circle);
    }

    public void OnCircleDeleted(CircleController deletedCircle)
    {
        // This means we got eaten
        if (OwnedCircles.Remove(deletedCircle) && IsLocalPlayer && OwnedCircles.Count == 0)
        {
            // DeathScreen.Instance.SetVisible(true);
        }
    }

    public int TotalMass()
    {
        return (int)OwnedCircles
            .Select(circle => GameManager.Conn.Db.Entity.EntityId.Find(circle.EntityId))
            .Sum(e => e?.Mass ?? 0); //If this entity is being deleted on the same frame that we're moving, we can have a null entity here.
    }

    public Vector2? CenterOfMass()
    {
        if (OwnedCircles.Count == 0)
        {
            return null;
        }

        Vector2 totalPos = Vector2.zero;
        float totalMass = 0;
        foreach (var circle in OwnedCircles)
        {
            var entity = GameManager.Conn.Db.Entity.EntityId.Find(circle.EntityId);
            var position = circle.transform.position;
            totalPos += (Vector2)position * entity.Mass;
            totalMass += entity.Mass;
        }

        return totalPos / totalMass;
    }

    private void OnGUI()
    {
        if (!IsLocalPlayer || !GameManager.IsConnected())
        {
            return;
        }

        GUI.Label(new Rect(0, 0, 100, 50), $"Total Mass: {TotalMass()}");
    }

    //Automated testing members
    private bool testInputEnabled;
    private Vector2 testInput;

    public void SetTestInput(Vector2 input) => testInput = input;
    public void EnableTestInput() => testInputEnabled = true;

    private static Vector2 CenterOfScreen()
    {
        return new Vector2(Screen.width, Screen.height) / 2;
    }

    private static Vector2 CurrentPointerPosition()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            return touchscreen.primaryTouch.position.ReadValue();
        }

        var mouse = Mouse.current;
        if (mouse != null)
        {
            return mouse.position.ReadValue();
        }

        return CenterOfScreen();
    }

    public void Update()
    {
        if (!IsLocalPlayer || NumberOfOwnedCircles == 0)
        {
            return;
        }

        var pointerPosition = CurrentPointerPosition();

        if (Keyboard.current?.qKey.wasPressedThisFrame == true)
        {
            if (LockInputPosition.HasValue)
            {
                LockInputPosition = null;
            }
            else
            {
                LockInputPosition = pointerPosition;
            }
        }

        // Throttled input requests
        if (Time.time - LastMovementSendTimestamp >= SEND_UPDATES_FREQUENCY)
        {
            LastMovementSendTimestamp = Time.time;

            pointerPosition = LockInputPosition ?? pointerPosition;
            var screenSize = new Vector2
            {
                x = Screen.width,
                y = Screen.height,
            };
            var centerOfScreen = screenSize / 2;

            var direction = (pointerPosition - centerOfScreen) / (screenSize.y / 3);
            if (testInputEnabled) { direction = testInput; }
            GameManager.Conn.Reducers.UpdatePlayerInput(direction);
        }
    }
}
