package com.example.dpsu.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.dpsu.data.local.entity.TransportPassEntity
import com.example.dpsu.data.repository.TransportRepository
import com.example.dpsu.domain.UserRole
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class TransportViewModel(
    private val repository: TransportRepository
) : ViewModel() {

    private val _items = MutableStateFlow<List<TransportPassEntity>>(emptyList())
    val items: StateFlow<List<TransportPassEntity>> = _items

    private var loadJob: Job? = null

    fun loadForRole(role: UserRole) {
        loadJob?.cancel()

        loadJob = viewModelScope.launch {
            repository.observeForRole(role).collect { list ->
                _items.value = list
            }
        }
    }

    fun createGreenTicket(
        vehicleNumber: String,
        vehicleType: String,
        personsCount: Int
    ) {
        viewModelScope.launch {
            repository.createGreenTicket(
                vehicleNumber = vehicleNumber,
                vehicleType = vehicleType,
                personsCount = personsCount
            )
        }
    }

    fun createRedTicket(
        vehicleNumber: String,
        vehicleType: String,
        personsCount: Int,
        trailerNumber: String?,
        fromLocation: String?,
        toLocation: String?
    ) {
        viewModelScope.launch {
            repository.createRedTicket(
                vehicleNumber = vehicleNumber,
                vehicleType = vehicleType,
                personsCount = personsCount,
                trailerNumber = trailerNumber,
                fromLocation = fromLocation,
                toLocation = toLocation
            )
        }
    }

    fun approve(passId: String, role: UserRole) {
        viewModelScope.launch {
            repository.approve(passId, role)
        }
    }

    fun reject(passId: String, role: UserRole) {
        viewModelScope.launch {
            repository.reject(passId, role)
        }
    }
}
