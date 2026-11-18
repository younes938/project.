package com.example.dpsu.ui.tyl

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.example.dpsu.ui.TransportViewModel

@Composable
fun GreenTicketScreen(
    viewModel: TransportViewModel,
    onSaved: () -> Unit,
    onBack: () -> Unit
) {
    var vehicleNumber by remember { mutableStateOf("") }
    var vehicleType by remember { mutableStateOf("") }
    var personsCount by remember { mutableStateOf("") }

    val persons = personsCount.toIntOrNull()
    val canSave = vehicleNumber.isNotBlank()
            && vehicleType.isNotBlank()
            && persons != null
            && persons > 0

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        verticalArrangement = Arrangement.Center
    ) {
        Card(
            modifier = Modifier.fillMaxWidth()
        ) {
            Column(
                modifier = Modifier.padding(32.dp),
                verticalArrangement = Arrangement.spacedBy(14.dp)
            ) {
                Text(
                    text = "Зелений талон",
                    style = MaterialTheme.typography.headlineMedium,
                    fontWeight = FontWeight.Bold
                )

                Text(
                    text = "Введіть основні дані транспортного засобу",
                    style = MaterialTheme.typography.titleMedium
                )

                Spacer(modifier = Modifier.height(8.dp))

                OutlinedTextField(
                    value = vehicleNumber,
                    onValueChange = { vehicleNumber = it.uppercase() },
                    label = { Text("Номер транспортного засобу") },
                    modifier = Modifier.fillMaxWidth()
                )

                OutlinedTextField(
                    value = vehicleType,
                    onValueChange = { vehicleType = it },
                    label = { Text("Тип транспортного засобу") },
                    modifier = Modifier.fillMaxWidth()
                )

                OutlinedTextField(
                    value = personsCount,
                    onValueChange = { value ->
                        personsCount = value.filter { it.isDigit() }
                    },
                    label = { Text("Кількість осіб") },
                    keyboardOptions = KeyboardOptions(
                        keyboardType = KeyboardType.Number
                    ),
                    modifier = Modifier.fillMaxWidth()
                )

                Button(
                    onClick = {
                        viewModel.createGreenTicket(
                            vehicleNumber = vehicleNumber,
                            vehicleType = vehicleType,
                            personsCount = persons ?: 0
                        )
                        onSaved()
                    },
                    enabled = canSave,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(56.dp)
                ) {
                    Text("Зберегти")
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
    }
}
