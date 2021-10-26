using UnityEngine;
using System.Collections;
/*
This script will use math to determine the nearest enemy and shoot at him automatically, no aim system just math.
CONTENT :
	- shoots on function call. 
	- Can be used with buttons, key presses.
	- Active target cursor UI.

1. Store in a list all enemies with Enemy tag 
2. Take the nearest enemy in the list
3. Checks if nearest enemy is in range
5. Puts a "active target cursor" on it.
4. Calls corresponding function for attacking (Shoot / Laser)
*/
public class PlayerAttack : MonoBehaviour {

	// Code stuff
	private Transform target;
	private enemyHealth targetEnemy;
	GameObject nearestEnemy;

	[Header("Bullet Attack")]
	public bool usingCountdown; // the script will do fireRate operations even if fireRate = 0, making the shooting system glitchy. This avoid making these operations and making it working if there's no fireRate.
	public string keyPressForAttack;
	public GameObject bulletPrefab;
	public float range;
	public float fireCountdown; // countdown between each shot, which is used to set the fireRate, internal modified value. This one is static
	//[HideInInspector]
	public float countdownTillNextBullet; // fireRate will be the one modified when shooting and doing countdowns between shots.

	[Header("Use Laser")]
	public bool useLaser;
	public int damageOverTime = 30;
	public LineRenderer lineRenderer;
	public Light impactLight;
	public GameObject impactEffect;

	[Header("Generic Fields")]
	GameObject targetCursor; // the Instantiated UI cursor from "targetCursorPrefab". Shows what's the active target that will be shot when we'll attack.
	public GameObject targetCursorPrefab; // same but prefab, usable one in script
	public bool usingTargetCursor;

	[Header("Unity Setup Fields")]
	public string enemyTag = "enemy";

	[Header("Attack by button/mouse")] // in some way, pc or mobile device
	public bool usingMouseInput; // This is to activate the shooting-by-clicking way. If not true, it means our platform is mobile device with a button to shoot, so we'll only accept button calls and not clicks, to call attack().

	[Header("Optional : specific firePoint (see script)")]
	public Transform firePoint; // optional, gives the possibility to have a specific firePoint. If no firePoint is defined, it will by default fire from the script's holder transform

	bool attackButtonPressed;

	public void Awake()
	{
		countdownTillNextBullet = fireCountdown;

		if(usingTargetCursor)	
		{
			targetCursor = Instantiate(targetCursorPrefab, this.transform.position, this.transform.rotation);
			targetCursor.SetActive(false);
		}	
	}

	void Update() 
	{
		if(usingMouseInput)
		{
			if (Input.GetMouseButtonDown(0))
    		{
				Debug.Log("Mouse Button is down, calling Attack()");
				Attack();
    		}
		}

		if (target == null)
		{
			if (useLaser)
			{
				if (lineRenderer.enabled)
				{
					lineRenderer.enabled = false;
					Destroy(impactEffect);
					impactLight.enabled = true;
				}
			}
		}

		if(usingCountdown && countdownTillNextBullet > 0)
		{
			countdownTillNextBullet -= Time.deltaTime;
		}

		UpdateTarget();
	}


	void UpdateTarget()
	{
		firePoint = this.transform;
		float shortestDistance = Mathf.Infinity;

		GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
		nearestEnemy = null;

		//Finding and storing nearest enemy (and its distance)
		foreach (GameObject enemy in enemies)
		{
			float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
			if (distanceToEnemy < shortestDistance) 
			{
				shortestDistance = distanceToEnemy;
				nearestEnemy = enemy;
			}
		}

		if (nearestEnemy != null && shortestDistance <= range)
		{
			target = nearestEnemy.transform;
			targetEnemy = nearestEnemy.GetComponent<enemyHealth>();
			
			if(usingTargetCursor)
			{
				targetCursor.SetActive(true);
				targetCursorTracking(target);
			}
		} 
		else
		{
			target = null;
			targetCursor.SetActive(false);
		}

	}


	public void Attack() 
	{
		if (useLaser)
		{
			Laser();
		} 
		else // if using bullets
		{
			if (countdownTillNextBullet <= 0f)
			{
				countdownTillNextBullet = fireCountdown;
				Shoot();
			}
		}
	}
	

	void Laser()
	{
		targetEnemy.TakeDamage(damageOverTime);
		//targetEnemy.TakeDamage(damageOverTime * Time.deltaTime);
		//targetEnemy.Slow(slowAmount);

		if (!lineRenderer.enabled)
		{
			lineRenderer.enabled = true;
			GameObject impactEffect = (GameObject)Instantiate(bulletPrefab, targetEnemy.transform.position, this.transform.rotation);
			impactLight.enabled = true;
		}

		lineRenderer.SetPosition(0, firePoint.position);
		lineRenderer.SetPosition(1, target.position);

		Vector3 dir = firePoint.position - target.position;

		impactEffect.transform.position = target.position + dir.normalized;

		impactEffect.transform.rotation = Quaternion.LookRotation(dir);
	}


	void Shoot()
	{
		GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
		Bullet bullet = bulletGO.GetComponent<Bullet>();

		if (bullet != null)
		{
			bullet.Init(target, nearestEnemy);
		}
	}

	void targetCursorTracking(Transform target)
	{
		targetCursor.transform.position = target.position;
	}

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, range);
	}
}