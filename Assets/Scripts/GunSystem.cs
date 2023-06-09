using UnityEngine;
using EZCameraShake;
using System.Collections;

public class GunSystem : MonoBehaviour
{
    [Header("Gun Stats")]
    public int damage;
    public float timeBetweenShooting,
        spread,
        range,
        reloadTime,
        timeBetweenShots;
    public int magazineSize,
        bulletsPerTap;
    public bool allowButtonHold;
    public float recoil;

    private int bulletsLeft,
        bulletsShot;
    private bool shooting,
        readyToShoot,
        reloading;

    [Header("References")]
    public Transform playerOrientation;
    public Camera fpsCamera;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask enemyLayer;

    [Header("Graphics")]
    public ParticleSystem muzzleFlash_GunCamera;
    public ParticleSystem muzzleFlash_MainCamera;
    public GameObject bulletHoleGraphic;
    public Transform gunModel;

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

        if (Input.GetKey(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
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
        playerOrientation.transform.rotation = playerOrientation.transform.rotation.normalized;

        StartCoroutine(Tremble());

        // Camera shake
        CameraShaker.Instance.ShakeOnce(0.5f, 4f, 0.08f, 0.08f);

        // spread
        float xRandSpread = Random.Range(-spread, spread);
        float yRandSpread = Random.Range(-spread, spread);

        Vector3 direction = fpsCamera.transform.forward + new Vector3(xRandSpread, yRandSpread, 0);

        // RayCast
        if (Physics.Raycast(fpsCamera.transform.position, direction, out rayHit, range, enemyLayer))
        {
            //Debug.Log("Hit : " + rayHit.transform.gameObject.name.ToString());
        }

        // bullet hole, muzzle flash
        Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.FromToRotation(Vector3.forward, rayHit.normal));
        muzzleFlash_GunCamera.Play();
        muzzleFlash_MainCamera.Play();

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
        bulletsLeft = magazineSize;
        reloading = false;
    }

    private IEnumerator Tremble()
    {
        for (int i = 0; i < 2; i++)
        {
            gunModel.transform.localPosition -= new Vector3(0.01f, 0.01f, 0.09f);
            yield return new WaitForSeconds(0.035f);
            gunModel.transform.localPosition += new Vector3(0.01f, 0.01f, 0.09f);
            yield return new WaitForSeconds(0.035f);
        }
    }

    public bool IsShooting()
    {
        return (!readyToShoot && !reloading && bulletsLeft > 0);
    }
}
