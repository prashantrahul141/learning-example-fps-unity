using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    [Header("Gun Stats")]
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    private int bulletsLeft, bulletsShot;
    private bool shooting, readyToShoot, reloading;

    [Header("References")]
    public Camera fpsCamera;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask enemyLayer;

    private void Start()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        TakeInput();
    }

    private void TakeInput()
    {
        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft > magazineSize && !reloading)
        {
            Reload();
        }

        // when to shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // spread
        float xRandSpread = Random.Range(-spread, spread);
        float yRandSpread = Random.Range(-spread, spread);

        Vector3 direction = fpsCamera.transform.forward + new Vector3(xRandSpread, yRandSpread, 0);

        // RayCast
        if (Physics.Raycast(fpsCamera.transform.position, direction, out rayHit, range, enemyLayer))
        {
            Debug.Log("Hit : " + rayHit.transform.gameObject.name.ToString());
        }

        bulletsLeft--;
        bulletsShot--;
        Invoke(nameof(ResetShot), timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
        {

            Invoke(nameof(Shoot), timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        reloading = false;
    }
}
