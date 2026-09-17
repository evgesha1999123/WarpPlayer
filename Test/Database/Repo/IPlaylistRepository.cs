using System;
using System.Collections.Generic;


namespace Test.Database.Repo
{
    public interface IPlaylistRepository<TPlaylist, TRecord> where TRecord: TPlaylist
    {
        public TRecord Get(Guid id);
        public IReadOnlyList<TRecord> GetMany(IReadOnlyCollection<Guid> ids);
        public IReadOnlyList<TRecord> GetAll();

        public TRecord Create(TPlaylist playlist);
        public IReadOnlyList<TRecord> CreateMany(IReadOnlyCollection<TPlaylist> playlist);

        public bool Delete(Guid id);
        public void DeleteMany(IReadOnlyCollection<Guid> ids);
        public void DeleteAll();
    }
}
