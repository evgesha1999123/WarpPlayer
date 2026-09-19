using System;
using System.Collections.Generic;


namespace Test.Repo
{
    public interface ITrackRepository<TFile, TRecord> where TRecord: TFile
    {
        public TRecord Get(Guid id);
        public IReadOnlyList<TRecord> GetMany(IReadOnlyCollection<Guid> guids);
        public IReadOnlyList<TRecord> GetAll();

        public TRecord Create(TFile file);
        public IReadOnlyList<TRecord> CreateMany(IReadOnlyCollection<TFile> files);

        public bool Delete(Guid id);
        public void DeleteMany(IReadOnlyCollection<Guid> ids);
        public void DeleteAll();
    }
}
