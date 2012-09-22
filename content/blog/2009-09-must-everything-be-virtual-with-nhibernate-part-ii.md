I already tried to explain this <a href="/blog/2009/03/must-everything-be-virtual-with-nhibernate/">before</a>, but here's a simple example from a presentation I recently did on NHibernate:  

<img src="/blog/wp-content/uploads/2009/09/transitive_persistence41.png" alt="transitive_persistence4" title="transitive_persistence4" width="798" height="635" class="aligncenter size-full wp-image-1647" />

As you can see, only the properties of associations that are eligible for lazy-loading are virtual in this piece of code, because that is what many people seem to want.   There are actually 2 different ways in which this can cause problems...  can you spot both problems?