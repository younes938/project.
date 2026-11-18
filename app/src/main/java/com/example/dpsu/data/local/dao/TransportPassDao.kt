package com.example.dpsu.data.local.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import androidx.room.Update
import com.example.dpsu.data.local.entity.TransportPassEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface TransportPassDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertTransportPass(pass: TransportPassEntity)

    @Update
    suspend fun updateTransportPass(pass: TransportPassEntity)

    @Query("SELECT * FROM transport_passes ORDER BY created_at DESC")
    fun observeAll(): Flow<List<TransportPassEntity>>

    @Query("SELECT * FROM transport_passes WHERE id = :id LIMIT 1")
    suspend fun getById(id: String): TransportPassEntity?

    @Query("""
        SELECT * FROM transport_passes
        WHERE current_stage = 'PD'
        ORDER BY created_at ASC
    """)
    fun observeForPD(): Flow<List<TransportPassEntity>>

    @Query("""
        SELECT * FROM transport_passes
        WHERE current_stage = 'DMS'
        AND pd_check = 1
        ORDER BY pd_check_at ASC
    """)
    fun observeForDMS(): Flow<List<TransportPassEntity>>

    @Query("""
        SELECT * FROM transport_passes
        WHERE current_stage = 'BORDER'
        AND pd_check = 1
        AND dms_check = 1
        ORDER BY dms_check_at ASC
    """)
    fun observeForBorder(): Flow<List<TransportPassEntity>>

    @Query("""
        SELECT * FROM transport_passes
        WHERE current_stage = 'COMPLETED'
        ORDER BY border_check_at DESC
    """)
    fun observeCompleted(): Flow<List<TransportPassEntity>>

    @Query("""
        SELECT * FROM transport_passes
        WHERE current_stage = 'STOPPED'
        ORDER BY created_at DESC
    """)
    fun observeStopped(): Flow<List<TransportPassEntity>>
}
