package com.example.dpsu.data.local.entity

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.PrimaryKey
import com.example.dpsu.domain.CurrentStage
import com.example.dpsu.domain.TicketType
import java.util.UUID

@Entity(tableName = "transport_passes")
data class TransportPassEntity(
    @PrimaryKey
    val id: String = UUID.randomUUID().toString(),

    @ColumnInfo(name = "ticket_type")
    val ticketType: TicketType,

    @ColumnInfo(name = "vehicle_number")
    val vehicleNumber: String,

    @ColumnInfo(name = "vehicle_type")
    val vehicleType: String,

    @ColumnInfo(name = "persons_count")
    val personsCount: Int,

    @ColumnInfo(name = "trailer_number")
    val trailerNumber: String? = null,

    @ColumnInfo(name = "from_location")
    val fromLocation: String? = null,

    @ColumnInfo(name = "to_location")
    val toLocation: String? = null,

    @ColumnInfo(name = "created_at")
    val createdAt: Long = System.currentTimeMillis(),

    @ColumnInfo(name = "pd_check")
    val pdCheck: Boolean? = null,

    @ColumnInfo(name = "pd_check_at")
    val pdCheckAt: Long? = null,

    @ColumnInfo(name = "dms_check")
    val dmsCheck: Boolean? = null,

    @ColumnInfo(name = "dms_check_at")
    val dmsCheckAt: Long? = null,

    @ColumnInfo(name = "border_check")
    val borderCheck: Boolean? = null,

    @ColumnInfo(name = "border_check_at")
    val borderCheckAt: Long? = null,

    @ColumnInfo(name = "current_stage")
    val currentStage: CurrentStage = CurrentStage.PD
)
