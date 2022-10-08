using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSwarm : MonoBehaviour {

    // I. Setup the PSO
	public float varhi = 100; 			// Upper limit on opt vars
	public float varlo = -100;				// Lower limit on opt vars

	public float maxVelocity = 10;

	// II. Stopping criteria
	public int maxit = 100;				// max number of iterations
	public float mincost = 0; 			// minimum cost

	// III. PSO parameters
	public int SwarmSize = 20;		   	// set population size
	public float c1 = 2f;		// set c1 (cognitive parameter)
	public float c2 = 2f;      // set c2 (social parameter)
	public float C =1f;	   	// constriction factor
	public float inertia_start = 0.9f;
	public float inertia_end = 0.4f;
	public float minBest = 10000000f;     //minimum cost found ever
	internal float minV;
	internal float maxV;


	public float waitTime = 2f;
	public Transform target;
    public GameObject gBestPosition;
	public GameObject prefab;
	internal List<GameObject> agentList = new List<GameObject>();
	public int gbestParticle = 0;  
  

	/// <summary>
	/// Starting 
	/// </summary>
	void Start()
	{   

		StartCoroutine("pso_algorithm"); 

	}


	// Particle Swarm Optimisation
	IEnumerator pso_algorithm ()  {
		/* Create random initial swarm */

		for (int i = 0; i < SwarmSize; i++) {
			float x_value = Random.value*(varhi-varlo) + varlo;
			float z_value = Random.value*(varhi-varlo) + varlo;
				GameObject clone = Instantiate (prefab, new Vector3 (x_value, 0f, z_value), transform.rotation) as GameObject;
				clone.transform.parent = this.transform;
			    clone.GetComponent<particle>().SetController (gameObject);
			    clone.GetComponent<particle>().localBestPosition = new Vector3 (Random.value*(varhi-varlo) + varlo, 0f, Random.value*(varhi-varlo) + varlo);
			    clone.GetComponent<particle>().particleVelocity = new Vector3 (Random.value*(2f * maxVelocity) - maxVelocity, 0f, Random.value*(2f * maxVelocity) - maxVelocity);

			     agentList.Add (clone);

				clone.GetComponent<Renderer>().material.color = Color.red;

			}

        // Calculates initial population costs using cost function 
        gBestPosition.GetComponent<Renderer>().material.color = Color.blue;

        for (int i = 0; i < SwarmSize; i++) {
			agentList[i].GetComponent<particle>().fittValue = CostF11 (agentList [i].transform.position);
			agentList[i].GetComponent<particle>().localBestfittValue =  CostF11 (agentList [i].transform.position);
			if (agentList [i].GetComponent<particle> ().fittValue < minBest) {
				minBest = agentList [i].GetComponent<particle> ().fittValue;
				gbestParticle = i;
       
            }

		}

		agentList [gbestParticle].GetComponent<Renderer> ().material.color = Color.green;
        gBestPosition.transform.position = agentList[gbestParticle].transform.position;


        int iga = 0;	            						// generation counter initialized
		while (iga < maxit) {
			iga++;
            yield return new WaitForSeconds(waitTime);
            //update inertia
            float inertia = inertia_start - (( inertia_start - inertia_end)/maxit) * (iga +1);
			//  update particle positions

			for (int i=0; i<SwarmSize; i++) {

					float r1 = Random.value; // random numbers
					float r2 = Random.value; // random numbers
				agentList[i].GetComponent<particle>().particleVelocity = inertia * agentList[i].GetComponent<particle>().particleVelocity + c1 * r1 * (agentList[i].GetComponent<particle>().localBestPosition - agentList[i].transform.position)
					+ c2 * r2* (gBestPosition.transform.position - agentList[i].transform.position);

				float x_0 = agentList [i].GetComponent<particle> ().particleVelocity.x;
				float z_0 = agentList [i].GetComponent<particle> ().particleVelocity.z;
				if (x_0 > maxVelocity)
					x_0 = maxVelocity;
				else if (x_0 < (-maxVelocity))
					x_0 = -maxVelocity;
				
				if (z_0 > maxVelocity)
					z_0 = maxVelocity;
				else if (z_0 < (-maxVelocity))
					z_0 = -maxVelocity;
				agentList [i].GetComponent<particle> ().particleVelocity = new Vector3 (x_0, 0f, z_0);

				x_0 = agentList[i].transform.position.x;
				z_0 = agentList [i].transform.position.z;

				agentList[i].transform.position = agentList[i].transform.position + agentList[i].GetComponent<particle>().particleVelocity; // updates particle position
				// make some limitspar,0
				float x_1 = agentList[i].transform.position.x;
				float z_1 = agentList[i].transform.position.z;
				if (x_1 < varlo)
					x_1 = varlo;
				else if (x_1 > varhi)
					x_1 = varhi;
				
				if (z_1 < varlo)
					z_1 = varlo;
				else if (z_1 > varhi)
					z_1 = varhi;

				agentList [i].transform.position = new Vector3 (x_1, 0f, z_1);
				agentList [i].transform.LookAt (new Vector3 (x_0,0f, z_0));
				}


            // Evaluate the new swarm
            gbestParticle = 0;
            for (int i = 0; i < SwarmSize; i++) {
                agentList[i].GetComponent<Renderer>().material.color = Color.red;
                agentList[i].GetComponent<particle>().fittValue = CostF11 (agentList [i].transform.position);

				// update local minimum for each particle
				if (agentList [i].GetComponent<particle> ().localBestfittValue > agentList[i].GetComponent<particle>().fittValue) {
					agentList [i].GetComponent<particle> ().localBestfittValue = CostF11 (agentList [i].transform.position);
					agentList [i].GetComponent<particle> ().localBestPosition = agentList [i].transform.position;
				}
                if (agentList[i].GetComponent<particle>().fittValue < agentList[gbestParticle].GetComponent<particle>().fittValue)
                {
                    gbestParticle = i;
                }
                // Finding best particle in the population (update global best)
                if (agentList [i].GetComponent<particle> ().fittValue < minBest) {
					minBest = agentList [i].GetComponent<particle> ().fittValue;
					
                    gBestPosition.transform.position = agentList[i].transform.position;
                  //  agentList[i].GetComponent<Renderer>().material.color = Color.green;
                }

			}

			//for (int i = 0; i < SwarmSize; i++) {
			//	agentList [i].GetComponent<Renderer> ().material.color = Color.red;
			//} 
			agentList [gbestParticle].GetComponent<Renderer> ().material.color = Color.green;

			yield return new WaitForSeconds(waitTime);
		} //end iteration
		print("best cost = " + minBest);
		print ("Solution is " + agentList [gbestParticle].transform.position);
		yield return new WaitForSeconds(waitTime);
		}//ENDofPSO
		
	//  distance to target 
	float CostF11(Vector3 pos) {
		
		Vector3 offset = new Vector3 (0f, 0f, 0f);

		float val = 0f; // fitness val 


		 offset = pos - target.transform.position;
		 val = offset.sqrMagnitude;

		  return  Mathf.Sqrt(val);

	}
} //End class