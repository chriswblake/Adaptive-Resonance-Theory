using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ch3_ART1_Clustering
{
    public partial class Form1 : Form
    {
        //Fields
        new ART1 art1 = new ART1();

        public Form1()
        {
            InitializeComponent();

            this.Height = 600;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> columns = new List<string>() {
                          "Hammer", "Paper", "Snickers", "ScrewDriver",  "Pen",  "Kit-Kat",  "Wrench",  "Pencil",  "Heath Bar",  "Tape Measure",  "Binder" };
            List<string> columnsShort = new List<string>() {
                          "Hmr", "Ppr", "Snk", "Scr",  "Pen",  "Kkt",  "Wrn",  "Pcl",  "Hth",  "Tpm",  "Bdr" };

            List<int[]> database = new List<int[]>
            {
                /*         Hmr  Ppr  Snk  Scr  Pen  Kkt  Wrn  Pcl  Hth  Tpm  Bdr */
                new int[] { 0,   0,   0,   0,   0,   1,   0,   0,   1,   0,   0}, //0
                new int[] { 0,   1,   0,   0,   0,   0,   0,   1,   0,   0,   1}, //1
                new int[] { 0,   0,   0,   1,   0,   0,   1,   0,   0,   1,   0}, //2
                new int[] { 0,   0,   0,   0,   1,   0,   0,   1,   0,   0,   1}, //3
                new int[] { 1,   0,   0,   1,   0,   0,   0,   0,   0,   1,   0}, //4
                new int[] { 0,   0,   0,   0,   1,   0,   0,   0,   0,   0,   1}, //5
                new int[] { 1,   0,   0,   1,   0,   0,   0,   0,   0,   0,   0}, //6
                new int[] { 0,   0,   1,   0,   0,   0,   0,   0,   1,   0,   0}, //7
                new int[] { 0,   0,   0,   0,   1,   0,   0,   1,   0,   0,   0}, //8
                new int[] { 0,   0,   1,   0,   0,   1,   0,   0,   1,   0,   0}  //9
            };

            //Analyzie datbase using ART1
            art1.addData(database);
            
            //Show clusters
            tbResults.Text += "CLUSTERS" + Environment.NewLine;
            tbResults.Text += "            " +  String.Join(" ", columnsShort.ToArray()) + Environment.NewLine;
            tbResults.Text += art1.getClusters();

            //Spacer
            tbResults.Text += Environment.NewLine;

            //Show recommendations
            tbResults.Text += "RECOMMENDATIONS" + Environment.NewLine;
            tbResults.Text += art1.getRecommendations();
        }
    }


    public class ART1
    {
        //Fields
        public List<FeatureVector> customers = new List<FeatureVector>();
        public List<Cluster> clusters = new List<Cluster>();
        
        double tieFactor = 1.0; //beta Tie factor (recommendation: small integer)
        double vigilenceFactor = 0.39; //rho, Vigilence factor (0 to 1)

        //Methods
        public void addData(List<int[]> inputCustomers)
        {
            //Convert customers into feature vectors
            int customerIndex = 0;
            foreach (int[] customer in inputCustomers)
            {
                this.customers.Add(new FeatureVector(customerIndex, customer));
                customerIndex++;
            }

            //Create Clusters
            createClusters();

            //Make Recommendations
            makeRecommendations();
        }
        private void createClusters()
        {
            //Repeat process until there are no more changes
            bool done = false; int limit = 50;
            while (!done)
            {
                //Assume done. If there is a change, this will be flipped.
                done = true;

                //Cycle through each customer
                foreach (FeatureVector customer in this.customers)
                {
                    //Compare to each cluster
                    foreach (Cluster cluster in clusters)
                    {
                        //Check for same cluster
                        if (cluster == customer.cluster)
                        { continue; }

                        //If passes proximity test
                        if (ProximityTest(cluster.featuresPrototype, customer.features))
                        {
                            //If passes vigilence test
                            if (VigilenceTest(cluster.featuresPrototype, customer.features))
                            {
                                //Record current cluster
                                Cluster oldCluster = customer.cluster;

                                //Move customer to new cluster
                                customer.cluster = cluster;

                                //Rebuild old cluster's prototype
                                if (oldCluster != null)
                                {
                                    //Get customers in old cluster
                                    List<FeatureVector> oldCustomers = customers.FindAll(c => c.cluster == oldCluster);

                                    //If there are none, delete this cluster
                                    if (oldCustomers.Count == 0) { clusters.Remove(oldCluster); }

                                    //Rebuild the old cluster's prototype
                                    if (oldCustomers.Count > 0) oldCluster.featuresPrototype = oldCustomers[0].features; //Reset to first item in list.
                                    foreach (FeatureVector c in oldCustomers)
                                    {
                                        oldCluster.featuresPrototype = BitwiseAnd(oldCluster.featuresPrototype, c.features);
                                    }
                                }

                                //Get customers in new cluster
                                List<FeatureVector> newCustomers = customers.FindAll(c => c.cluster == cluster);

                                //Rebuild new cluster's prototype
                                if (newCustomers.Count > 0) cluster.featuresPrototype = newCustomers[0].features; //Reset to first item in list.
                                foreach (FeatureVector c in newCustomers)
                                {
                                    cluster.featuresPrototype = BitwiseAnd(cluster.featuresPrototype, c.features);
                                }

                                //A change was found, so required one more pass
                                done = false;
                                break;
                            }
                        }
                    }

                    //Create a prototype for customers that do not match an existing prototype
                    if (customer.cluster == null)
                    {
                        //Create new cluster and add to list
                        Cluster newCluster = new Cluster(customer);
                        clusters.Add(newCluster);

                        //Set the customer to use this cluster
                        customer.cluster = newCluster;

                        //Keep processing, as something changed.
                        done = false;
                    }
                }

                //Check limit
                limit--;
                if (limit == 0) break;
            }
        }
        private void makeRecommendations()
        {
            foreach (FeatureVector customer in customers)
            {
                //Clear current customer recomendation
                customer.recommendation = new int[customer.features.Length];

                //Get all other customers in the same cluster
                foreach(FeatureVector clusterMember in customers.FindAll(cm=> cm!= customer && cm.cluster == customer.cluster))
                {
                    //Go through each feature
                    for(int f =0; f< customer.features.Length; f++)
                    {
                        //If the customer is missing this feature, count how many of the cluster members have it.
                        if (customer.features[f] == 0)
                        { customer.recommendation[f] += clusterMember.features[f]; }
                    }
                }
            }
        }

        //Methods - Display
        public string getClusters()
        {
            string s = "";

            //Clusters
            foreach (Cluster cluster in clusters)
            {
                //Show prototype vector
                s += " Prototype: " + itemToString(cluster.featuresPrototype, "   ") + Environment.NewLine;

                //Show members
                foreach (FeatureVector customer in customers.FindAll(c => c.cluster == cluster))
                {
                    s += "Customer " + customer.index + ": " + itemToString(customer.features, "   ") + Environment.NewLine;
                }

                //Drop down a line
                s += Environment.NewLine; ;

            }

            return s;
        }
        public string getRecommendations()
        {
            string s = "";
            foreach (FeatureVector customer in customers)
            {
                //Show recommendation vector
                s += "Customer " + customer.index + ": " + itemToString(customer.recommendation, "   ");

                if (customer.recommendation.Sum() != 0)
                {
                    s += "(Item " + customer.recommendation.ToList().IndexOf(customer.recommendation.Max()) + ")" + Environment.NewLine;
                }
                else
                {
                    s += "(None)";
                }
            }

            return s;
        }
        private string itemToString(int[] item, string del)
        {
            string s = "";
            foreach (int i in item)
            {
                s += i + del;
            }

            return s;
        }

        //Methods - Processing
        private bool ProximityTest(int[] prototype, int[] newItem)
        {
            //Check that dimensions agree
            if (newItem.Length != prototype.Length)
            { throw new ArgumentException("The features vectors must be the same size."); }

            //Calculate comparison
            double leftSide = ((double)BitwiseAnd(newItem, prototype).Sum()) / (double)(tieFactor + prototype.Sum());
            double rightSide = ((double)newItem.Sum()) / (tieFactor + prototype.Length);

            //Compute comparison
            return leftSide > rightSide;
        }
        private bool VigilenceTest(int[] prototype, int[] newItem)
        {
            //Calculate the bitwise AND comparison and compare it to the number of total in the newItem.
            double vigCalculation = ((double)BitwiseAnd(newItem, prototype).Sum()) / newItem.Sum();

            //If the calculation is greater than the vigilence factor, pass back true.
            return vigCalculation < vigilenceFactor;
        }
        private int[] BitwiseAnd(int[] A, int[] B)
        {
            //Check that dimensions agree
            if (A.Length != B.Length)
            { throw new ArgumentException("The features vectors must be the same size."); }

            //Compare
            int[] result = new int[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                if (A[i] == 1 && B[i] == 1)
                { result[i] = 1; }
            }

            //Return bitwise AND comparison
            return result;
        }

    }
    public class Cluster
    {
        //Fields
        public int[] featuresPrototype;

        //Constructor
        public Cluster(FeatureVector fv)
        {
            this.featuresPrototype = fv.features;
        }

        //Property
        public string prototypeAsString
        {
            get
            {
                string s = "";
                foreach (int i in featuresPrototype)
                {
                    s += i + " ";
                }

                return s;
            }
        }
    }
    public class FeatureVector
    {
        //Fields
        public int index;
        public Cluster cluster = null;
        public int[] features;
        public int[] recommendation;

        //Indexer
        public int this[int index]
        {
            get { return features[index]; }
            set { features[index] = value; }
        }

        //Constructor
        public FeatureVector(int index, int[] features)
        {
            this.index = index;
            this.features = features;
        }

        //Properties
        public Cluster Cluster
        {
            get { return cluster; }
            set { cluster = value; }
        }
    }
}
