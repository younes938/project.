package com.example.dpsu.data.repository

import com.example.dpsu.data.local.dao.TransportPassDao
import com.example.dpsu.data.local.entity.TransportPassEntity
import com.example.dpsu.domain.CurrentStage
import com.example.dpsu.domain.TicketType
import com.example.dpsu.domain.UserRole
import kotlinx.coroutines.flow.Flow

class TransportRepository(
    private val dao: TransportPassDao
) {

    fun observeForRole(role: UserRole): Flow<List<TransportPassEntity>> {
        return when (role) {
            UserRole.VSH_TYL -> dao.observeAll()
            UserRole.PD -> dao.observeForPD()
            UserRole.DMS -> dao.observeForDMS()
            UserRole.VSH_KORDON -> dao.observeForBorder()
        }
    }

    suspend fun createGreenTicket(
        vehicleNumber: String,
        vehicleType: String,
        personsCount: Int
    ) {
        val pass = TransportPassEntity(
            ticketType = TicketType.GREEN,
            vehicleNumber = vehicleNumber.trim(),
            vehicleType = vehicleType.trim(),
            personsCount = personsCount,
            currentStage = CurrentStage.PD
        )

        dao.insertTransportPass(pass)
    }

    suspend fun createRedTicket(
        vehicleNumber: String,
        vehicleType: String,
        personsCount: Int,
        trailerNumber: String?,
        fromLocation: String?,
        toLocation: String?
    ) {
        val pass = TransportPassEntity(
            ticketType = TicketType.RED,
            vehicleNumber = vehicleNumber.trim(),
            vehicleType = vehicleType.trim(),
            personsCount = personsCount,
            trailerNumber = trailerNumber?.trim()?.ifBlank { null },
            fromLocation = fromLocation?.trim()?.ifBlank { null },
            toLocation = toLocation?.trim()?.ifBlank { null },
            currentStage = CurrentStage.PD
        )

        dao.insertTransportPass(pass)
    }

    suspend fun approve(passId: String, role: UserRole) {
        val pass = dao.getById(passId) ?: return
        val now = System.currentTimeMillis()

        val updatedPass = when (role) {
            UserRole.PD -> pass.copy(
                pdCheck = true,
                pdCheckAt = now,
                currentStage = CurrentStage.DMS
            )

            UserRole.DMS -> pass.copy(
                dmsCheck = true,
                dmsCheckAt = now,
                currentStage = CurrentStage.BORDER
            )

            UserRole.VSH_KORDON -> pass.copy(
                borderCheck = true,
                borderCheckAt = now,
                currentStage = CurrentStage.COMPLETED
            )

            UserRole.VSH_TYL -> pass
        }

        dao.updateTransportPass(updatedPass)
    }

    suspend fun reject(passId: String, role: UserRole) {
        val pass = dao.getById(passId) ?: return
        val now = System.currentTimeMillis()

        val updatedPass = when (role) {
            UserRole.PD -> pass.copy(
                pdCheck = false,
                pdCheckAt = now,
                currentStage = CurrentStage.STOPPED
            )

            UserRole.DMS -> pass.copy(
                dmsCheck = false,
                dmsCheckAt = now,
                currentStage = CurrentStage.STOPPED
            )

            UserRole.VSH_KORDON -> pass.copy(
                borderCheck = false,
                borderCheckAt = now,
                currentStage = CurrentStage.STOPPED
            )

            UserRole.VSH_TYL -> pass
        }

        dao.updateTransportPass(updatedPass)
    }
}
