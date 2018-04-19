//Program Constants
#define MAX_ITEMS                         (11)
#define MAX_CUSTOMERS                     (10)
#define TOTAL_PROTOTYPE_VECTORS           (5)

//Configuration parameters
const float beta = 1.0; //Tie Factor, small positive number
const float vigilance = 0.9; // 0 <= vigilance < 1

//Prototype Vectors
int numPrototypeVectors = 0;
int prototypeVector[TOTAL_PROTOTYPE_VECTORS][MAX_ITEMS];
int members[TOTAL_PROTOTYPE_VECTORS];

//Customer and cluster membership
int membership[MAX_CUSTOMERS];

// For making Recommendations
int sumVector[TOTAL_PROTOTYPE_VECTORS][MAX_ITEMS];

/*
 * Feature vectors are contained within the database array. A one in
 * the field represents a product that the customer has purchased. A
 * zero represents a product not purchased by the customer.
 */
/*       Hmr  Ppr  Snk  Scr  Pen  Kkt  Wrn  Pcl  Hth  Tpm  Bdr */
int database[MAX_CUSTOMERS][MAX_ITEMS] = {
        { 0,   0,   0,   0,   0,   1,   0,   0,   1,   0,   0},
        { 0,   1,   0,   0,   0,   0,   0,   1,   0,   0,   1},
        { 0,   0,   0,   1,   0,   0,   1,   0,   0,   1,   0},
        { 0,   0,   0,   0,   1,   0,   0,   1,   0,   0,   1},
        { 1,   0,   0,   1,   0,   0,   0,   0,   0,   1,   0},
        { 0,   0,   0,   0,   1,   0,   0,   0,   0,   0,   1},
        { 1,   0,   0,   1,   0,   0,   0,   0,   0,   0,   0},
        { 0,   0,   1,   0,   0,   0,   0,   0,   1,   0,   0},
        { 0,   0,   0,   0,   1,   0,   0,   1,   0,   0,   0},
        { 0,   0,   1,   0,   0,   1,   0,   0,   1,   0,   0}
};



int main()
{
  int customer;

  srand( time( NULL ) );

  initialize();

  //Perform analysis
  performART1();

  //Show customers
  displayCustomerDatabase();

  //Display recommendations
  for (customer = 0 ; customer < MAX_CUSTOMERS ; customer++)
  {
	  makeRecommendation( customer );
  }

  return 0;
}

void initialize( void )
{
	int i, j;

	//Clear prototype vectors 
	for (i = 0 ; i < TOTAL_PROTOTYPE_VECTORS ; i++)
	{
		for (j = 0 ; j < MAX_ITEMS ; j++)
		{
			prototypeVector[i][j] = 0;
			sumVector[i][j] = 0;
		}
		members[i] = 0;
	}

	//Initialize example vectors to no membership to any cluster
	for (j = 0 ; j < MAX_CUSTOMERS ; j++)
	{
		membership[j] = -1;
	}

}

int performART1(void)
{
	int andresult[MAX_ITEMS];
	int pvec, magPE, magP, magE;
	float result, test;
	int index, done = 0;
	int count = 50;

	while (!done)
	{
		done = 1;

		//Cycle through each customer
		for (index = 0; index < MAX_CUSTOMERS; index++)
		{
			//Compare to each prototype vector
			for (pvec = 0; pvec < TOTAL_PROTOTYPE_VECTORS; pvec++)
			{
				// Does this vector have any members?
				if (members[pvec])
				{
					//Get and result of current customer and prototype vector
					vectorBitwiseAnd(andresult, &database[index][0], &prototypeVector[pvec][0]);

					//Magnitude of bitwise compare
					magPE = vectorMagnitude(andresult);

					//Magnitude of prototype vector
					magP = vectorMagnitude(&prototypeVector[pvec][0]);

					//Magnitude of customer vector
					magE = vectorMagnitude(&database[index][0]);

					//Proximity check values
					result = (float)magPE / (beta + (float)magP); //left side
					test = (float)magE / (beta + (float)MAX_ITEMS); //right side

					//If passes proximity test
					if (result > test)
					{
						//If passes vigilence test
						if (((float)magPE / (float)magE) < vigilance)
						{
							//Only for a different cluster
							if (membership[index] 1= pvec)
							{
								//Move the customer to the new cluster
								int old = membership[index];
								membership[index] = pvec;
								members[pvec]++;

								//Check if there are any more items in this older cluster.
								//If there aren't reduce the count of the prototype vectors
								if (old >= 0)
								{
									members[old]-;
									if (members[old] == 0)
										numPrototypeVectors-;
								}
								
								//Recalculate the prototype vector for the old cluster
								if ((old >= 0) && (old < TOTAL_PROTOTYPE_VECTORS))
								{
									updatePrototypeVectors(old);
								}

								//Recalculate the prototype vector for the new cluster
								updatePrototypeVectors(pvec);

								//The new prototype vector was found. End the loop.
								done = 0;
								break;
							}
							else { } // Already in this cluster
						}
					}
				}
			}

			//Create a prototype for customers that do not match an existing prototype
			if (membership[index] == -1)
			{			
				membership[index] = createNewPrototypeVector(&database[index][0]);
				done = 0;
			}
		}

		if (!count-) break;
	}

	return 0;
}

void makeRecommendation(int customer)
{
	int bestItem = -1;
	int val = 0;
	int item;

	//Cycle through each customer
	for (item = 0; item < MAX_ITEMS; item++)
	{
		//Retrieve sum vector for unpurchase items of the customer. Pick index of highest item.
		if ((database[customer][item] == 0) && (sumVector[membership[customer]][item] > val))
		{
			bestItem = item;
			val = sumVector[membership[customer]][item];
		}
	}

	//Show customer
	printf("For Customer %d, ", customer);

	//Show recommendation
	if (bestItem >= 0)
	{
		printf("The best recommendation is %d (%s)\n", bestItem, itemName[bestItem]);
		printf("Owned by %d out of %d members of this cluster\n", sumVector[membership[customer]][bestItem], members[membership[customer]]);
	}
	else
	{
		printf("No recommendation can be made.\n");
	}

	//Show previous purchases
	printf("Already owns: ");
	for (item = 0; item < MAX_ITEMS; item++)
	{
		if (database[customer][item]) printf("%s ", itemName[item]);
	}
	printf("\n\n");
}

//Methods
int vectorMagnitude( int *vector )
{
	int j, total = 0;

	for (j = 0 ; j < MAX_ITEMS ; j++)
	{
		if (vector[j] == 1)  total++;
	}

	return total;
}
void vectorBitwiseAnd( int *result, int *v, int *w )
{
	int i;
	for (i = 0 ; i < MAX_ITEMS ; i++)
	{
		result[i] = (v[i] && w[i]);
	}

	return;
}
int createNewPrototypeVector( int *example )
{
	int i, cluster;

	//Get next available index in "members" vector
	for (cluster = 0 ; cluster < TOTAL_PROTOTYPE_VECTORS ; cluster++)
	{
		if (members[cluster] == 0)
			break;
	}

	//Check if at the the limit
	if (cluster == TOTAL_PROTOTYPE_VECTORS)
		assert(0);
	
	//Keep track of number of prototype vectors
	numPrototypeVectors++;

	//Add the example to the prototype vector
	for (i = 0 ; i < MAX_ITEMS ; i++)
	{
		prototypeVector[cluster][i] = example[i];
	}

	//Set the initial membership to 1.
	members[cluster] = 1;

	return cluster;
}
void updatePrototypeVectors( int cluster )
{
    int item, customer, first = 1;

	//Require a positive index value
    assert( cluster >= 0);

	//Clear existing prototype vector and sum vector
    for (item = 0 ; item < MAX_ITEMS ; item++)
	{
		prototypeVector[cluster][item] = 0;
		sumVector[cluster][item] = 0;
    }

	//Cycle through each customer
    for (customer = 0 ; customer < MAX_CUSTOMERS ; customer++)
	{
		//Check if customer belongs to this cluster
		if (membership[customer] == cluster)
		{
			if (first)
			{
				//Copy first item directly to the prototype vector
				for (item = 0 ; item < MAX_ITEMS ; item++)
				{
					prototypeVector[cluster][item] = database[customer][item];
					sumVector[cluster][item] = database[customer][item];
				}

				//Mark that first is completed
				first = 0;
			} else
			{
				//Combine all additional customers to the prototype via BitwiseAnd
				for (item = 0 ; item < MAX_ITEMS ; item++)
				{
					prototypeVector[cluster][item] = prototypeVector[cluster][item] && database[customer][item];
					sumVector[cluster][item] += database[customer][item];
				}
			}
		}
	}

	return;
}