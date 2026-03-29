using TMPro;
using UnityEngine;

public class PeriodWindStream : MonoBehaviour
{
    [Header("Wind Settings")]
    [SerializeField] private ParticleSystem _visual;
    public Vector2 WindDirection = Vector2.right; 
    public float WindForce = 15f;
    public float WindDuration = 4f;
    public float CoolDown = 4f;
    private float _timer = 0f;
    private bool _isActive;

    [Header("Color Setting")]
    public bool CustomColor;
    public Color WindColor;

    private void Start()
    {
        InitParticleSettings();
    }
    private void Update()
    {
        _timer += Time.deltaTime;
        if(_isActive == false && _timer > CoolDown)
        {
            _timer = 0f;
            _isActive = true;
            _visual.Play();
        }
        else if(_isActive == true && _timer > WindDuration)
        {
            _timer = 0f;
            _isActive = false;
            _visual.Stop();
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(!_isActive) return;
            PlayerController player = collision.GetComponent<PlayerController>();
            
            if (player != null)
            {
                player.ApplyWindForce(WindDirection.normalized * WindForce);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplyWindForce(Vector2.zero); 

                if (player.Rb.linearVelocity.y > 1f)
                {
                    player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, player.Rb.linearVelocity.y * 0.5f);
                }
            }
        }
    }

    private void InitParticleSettings()
    {
        if (_visual == null) return;

        var emissionModule = _visual.emission;
        var velocityModule = _visual.velocityOverLifetime;
        var mainModule = _visual.main;

        emissionModule.rateOverTime = WindForce * 1.5f;

        velocityModule.x = WindDirection.x * WindForce;
        velocityModule.y = WindDirection.y * WindForce;

        if(CustomColor)
        {
            return;
        }
        if (WindDirection.y > 0.5f) 
        {
            mainModule.startColor = new Color(0.4f, 0.9f, 1f, 0.5f); 
        }
        else if (WindDirection.x != 0 && WindForce >= 15f)
        {
            mainModule.startColor = new Color(1f, 0.6f, 0.4f, 0.5f);
        }
        else 
        {
            mainModule.startColor = new Color(1f, 1f, 1f, 0.4f); 
        }
    }
}