using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DriverCar : MonoBehaviour
{
    [SerializeField] private int direction;
    [SerializeField] private float gasoline = 1;
    [SerializeField] private float gastoExp = 0.1f;
    [SerializeField] private Image fill;
    [SerializeField] private Rigidbody2D _frontTireRB;
    [SerializeField] private Rigidbody2D _backTireRB;
    [SerializeField] private Rigidbody2D _carRB;
    [SerializeField] private float _speed = 150f;
    [SerializeField] private float _rotationSpeed = 300f;

    private void Update()
    {
        UpdateFuelGauge();
    }

    private void FixedUpdate()
    {
        MoveMobile();
        ConsumeLogic();
    }

    private void UpdateFuelGauge()
    {
        fill.fillAmount = gasoline / 1;
    }

    private void MoveMobile()
    {
        _frontTireRB.AddTorque(-direction * _speed * Time.deltaTime);
        _backTireRB.AddTorque(-direction * _speed * Time.deltaTime);
        _carRB.AddTorque(direction * _rotationSpeed * Time.deltaTime);
    }

    private void ConsumeLogic()
    {
        if (gasoline > 0)
        {
            gasoline -= gastoExp * Mathf.Abs(direction) * Time.fixedDeltaTime;
            UpdateFuelGauge();
        }
        else
        {
            gasoline = 0;
            UpdateFuelGauge();
        }
    }

    public void Refuel(float amount)
    {
        gasoline = Mathf.Clamp(gasoline + amount, 0, 1);
        UpdateFuelGauge();
    }

    public void carInputMobile(int dir)
    {
        direction = dir;
    }

    public void GasButtonDown()
    {
        carInputMobile(1);
    }

    public void GasButtonUp()
    {
        carInputMobile(0);
    }

    public void BrakeButtonDown()
    {
        carInputMobile(-1);
    }

    public void BrakeButtonUp()
    {
        carInputMobile(0);
    }
}
