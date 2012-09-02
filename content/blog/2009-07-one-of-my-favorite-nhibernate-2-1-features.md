Check out the following piece of code:

<pre>
<code>
            Session.CreateQuery(
                @"delete from DocumentTypeAssignment 
                  where Id.DmsDocument in (from DmsDocument where Id = :documentId) and 
                        Id.DocumentType.Id not in (:newDocumentTypeIds)")
                .SetInt64("documentId", dmsDocumentId)
                .SetParameterList("newDocumentTypeIds", newDocumentTypesToAssign.ToList(), NHibernateUtil.Int64)
                .ExecuteUpdate();
</code>
</pre>

(pay no attention to Id.DmsDocument or Id.DocumentType... it's a composite key for a legacy table)

Which results in this SQL statement:

<pre>
<code>
delete 
    from
        DocumentManagement.DocumentTypeAssignment 
    where
        (
            DmsDocumentID in (
                select
                    dmsdocumen1_.ID 
                from
                    DocumentManagement.DmsDocument dmsdocumen1_ 
                where
                    dmsdocumen1_.ID=@p0
            )
        ) 
        and (
            DocumentTypeID not in  (
                @p1 , @p2
            )
        );
    @p0 = 1634, @p1 = 2313, @p2 = 2310
</code>
</pre>