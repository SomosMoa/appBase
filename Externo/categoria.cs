using System;
using Google.Cloud.Firestore;

namespace apiBillFold.Externo
{
    [FirestoreData]

    public class categoria
    {
        [FirestoreProperty]
        public string Categoria { get; set; }
        [FirestoreProperty]

        public string Id { get; set; }
        [FirestoreProperty]

        public string imagen { get; set; }

        [FirestoreProperty]
        public string tienda { get; set; }
        public categoria()
        {

        }
    }
}

