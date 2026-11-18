package com.example.dpsu.ui.checkpoint

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
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
fun TransportDetailsScreen(
    role: UserRole,
    passId: String,
    viewModel: TransportViewModel,
    onBack: () -> Unit,
    onActionDone: () -> Unit
) {
    val items by viewModel.items.collectAsState()
    val pass = items.firstOrNull { it.id == passId }

    if (pass == null) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(32.dp),
            verticalArrangement = Arrangement.Center
        ) {
            Text(
                text = "Запис не знайдено",
                style = MaterialTheme.typography.headlineSmall
            )

            Spacer(modifier = Modifier.height(16.dp))

            OutlinedButton(
                onClick = onBack,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Назад")
            }
        }
        return
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp)
            .verticalScroll(rememberScrollState())
    ) {
        Text(
            text = "Деталі транспортного засобу",
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold
        )

        Spacer(modifier = Modifier.height(16.dp))

        Card(
            modifier = Modifier.fillMaxWidth()
        ) {
            Column(
                modifier = Modifier.padding(24.dp),
                verticalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                InfoRow("Номер ТЗ", pass.vehicleNumber)
                InfoRow("Тип ТЗ", pass.vehicleType)
                InfoRow("Кількість осіб", pass.personsCount.toString())
                InfoRow(
                    "Тип талону",
                    if (pass.ticketType == TicketType.GREEN) "Зелений" else "Червоний"
                )

                if (pass.ticketType == TicketType.RED) {
                    InfoRow("Номер причепа", pass.trailerNumber ?: "—")
                    InfoRow("Звідки", pass.fromLocation ?: "—")
                    InfoRow("Куди", pass.toLocation ?: "—")
                }

                InfoRow("Дата створення", DateUtils.formatDateTime(pass.createdAt))
            }
        }

        Spacer(modifier = Modifier.height(16.dp))

        Card(
            modifier = Modifier.fillMaxWidth()
        ) {
            Column(
                modifier = Modifier.padding(24.dp),
                verticalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                Text(
                    text = "Статуси перевірки",
                    style = MaterialTheme.typography.titleLarge,
                    fontWeight = FontWeight.Bold
                )

                InfoRow("ПД", checkStatus(pass.pdCheck))
                InfoRow("Час ПД", DateUtils.formatDateTime(pass.pdCheckAt))

                InfoRow("ДМС", checkStatus(pass.dmsCheck))
                InfoRow("Час ДМС", DateUtils.formatDateTime(pass.dmsCheckAt))

                InfoRow("ВШ Кордон", checkStatus(pass.borderCheck))
                InfoRow("Час ВШ Кордон", DateUtils.formatDateTime(pass.borderCheckAt))

                InfoRow("Поточний етап", stageStatus(pass))
            }
        }

        Spacer(modifier = Modifier.height(24.dp))

        if (role != UserRole.VSH_TYL) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                Button(
                    onClick = {
                        viewModel.approve(pass.id, role)
                        onActionDone()
                    },
                    modifier = Modifier
                        .weight(1f)
                        .height(56.dp)
                ) {
                    Text("Підтвердити")
                }

                OutlinedButton(
                    onClick = {
                        viewModel.reject(pass.id, role)
                        onActionDone()
                    },
                    modifier = Modifier
                        .weight(1f)
                        .height(56.dp)
                ) {
                    Text("Відхилити")
                }
            }

            Spacer(modifier = Modifier.height(12.dp))
        }

        OutlinedButton(
            onClick = onBack,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp)
        ) {
            Text("Назад")
        }
    }
}

@Composable
private fun InfoRow(
    title: String,
    value: String
) {
    Column(
        modifier = Modifier.fillMaxWidth()
    ) {
        Text(
            text = title,
            style = MaterialTheme.typography.labelLarge,
            fontWeight = FontWeight.Bold
        )

        Text(
            text = value,
            style = MaterialTheme.typography.bodyLarge
        )
    }
}

private fun checkStatus(value: Boolean?): String {
    return when (value) {
        true -> "Підтверджено"
        false -> "Відхилено"
        null -> "Не перевірено"
    }
}

private fun stageStatus(pass: TransportPassEntity): String {
    return when (pass.currentStage.name) {
        "PD" -> "Очікує ПД"
        "DMS" -> "Очікує ДМС"
        "BORDER" -> "Очікує ВШ Кордон"
        "COMPLETED" -> "Завершено"
        "STOPPED" -> "Зупинено"
        else -> pass.currentStage.name
    }
}
