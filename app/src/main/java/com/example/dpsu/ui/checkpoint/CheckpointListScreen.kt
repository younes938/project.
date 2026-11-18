package com.example.dpsu.ui.checkpoint

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.example.dpsu.data.local.entity.TransportPassEntity
import com.example.dpsu.domain.TicketType
import com.example.dpsu.domain.UserRole
import com.example.dpsu.ui.TransportViewModel
import com.example.dpsu.utils.DateUtils

@Composable
fun CheckpointListScreen(
    role: UserRole,
    viewModel: TransportViewModel,
    onItemClick: (String) -> Unit,
    onBack: () -> Unit
) {
    LaunchedEffect(role) {
        viewModel.loadForRole(role)
    }

    val items by viewModel.items.collectAsState()

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp)
    ) {
        Text(
            text = screenTitle(role),
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold
        )

        Text(
            text = screenSubtitle(role),
            style = MaterialTheme.typography.titleMedium
        )

        Spacer(modifier = Modifier.height(16.dp))

        OutlinedButton(
            onClick = onBack,
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Назад")
        }

        Spacer(modifier = Modifier.height(16.dp))

        if (items.isEmpty()) {
            Card(
                modifier = Modifier.fillMaxWidth()
            ) {
                Column(
                    modifier = Modifier.padding(24.dp)
                ) {
                    Text(
                        text = "Записів немає",
                        style = MaterialTheme.typography.titleMedium
                    )
                    Text(
                        text = "На цьому етапі поки немає транспортних засобів для обробки.",
                        style = MaterialTheme.typography.bodyMedium
                    )
                }
            }
        } else {
            LazyColumn(
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                items(items) { pass ->
                    TransportPassListItem(
                        pass = pass,
                        onClick = {
                            onItemClick(pass.id)
                        }
                    )
                }
            }
        }
    }
}

@Composable
private fun TransportPassListItem(
    pass: TransportPassEntity,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onClick() }
    ) {
        Column(
            modifier = Modifier.padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Text(
                    text = pass.vehicleNumber,
                    style = MaterialTheme.typography.titleLarge,
                    fontWeight = FontWeight.Bold
                )

                Text(
                    text = if (pass.ticketType == TicketType.GREEN) "Зелений" else "Червоний",
                    style = MaterialTheme.typography.titleMedium
                )
            }

            Text("Тип ТЗ: ${pass.vehicleType}")
            Text("Кількість осіб: ${pass.personsCount}")
            Text("Створено: ${DateUtils.formatDateTime(pass.createdAt)}")
            Text("Поточний етап: ${stageTitle(pass.currentStage.name)}")
        }
    }
}

private fun screenTitle(role: UserRole): String {
    return when (role) {
        UserRole.VSH_TYL -> "Всі створені записи"
        UserRole.PD -> "ПД"
        UserRole.DMS -> "ДМС"
        UserRole.VSH_KORDON -> "ВШ Кордон"
    }
}

private fun screenSubtitle(role: UserRole): String {
    return when (role) {
        UserRole.VSH_TYL -> "Перегляд транспортних засобів"
        UserRole.PD -> "Очікують перевірку ПД"
        UserRole.DMS -> "Очікують перевірку ДМС"
        UserRole.VSH_KORDON -> "Очікують підтвердження ВШ Кордон"
    }
}

private fun stageTitle(stage: String): String {
    return when (stage) {
        "PD" -> "Очікує ПД"
        "DMS" -> "Очікує ДМС"
        "BORDER" -> "Очікує ВШ Кордон"
        "COMPLETED" -> "Завершено"
        "STOPPED" -> "Зупинено"
        else -> stage
    }
}
