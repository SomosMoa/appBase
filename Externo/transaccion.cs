using System;
using Google.Cloud.Firestore;

namespace apiBillFold.Externo
{

    [FirestoreData]

    public class transaccion
    {
        public transaccion()
        {
        }
        [FirestoreProperty]
        public string id { get; set; }
        [FirestoreProperty]
        public string transactionName { get; set; }
        [FirestoreProperty]
        public object transactionTime { get; set; }
        [FirestoreProperty]
        public string transactionPlace { get; set; }
        [FirestoreProperty]
        public DocumentReference user { get; set; }
        [FirestoreProperty]
        public string transactionReason { get; set; }
        [FirestoreProperty]
        public int NoFactura { get; set; }
        [FirestoreProperty]
        public double transactionAmount { get; set; }
        [FirestoreProperty]
        public string categoria { get; set; }
    }
}

