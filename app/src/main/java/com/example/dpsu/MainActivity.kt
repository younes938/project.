package com.example.dpsu

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.material3.MaterialTheme
import com.example.dpsu.data.local.DatabaseProvider
import com.example.dpsu.data.repository.TransportRepository
import com.example.dpsu.ui.TransportViewModel
import com.example.dpsu.ui.navigation.AppNavigation

class MainActivity : ComponentActivity() {

    private lateinit var transportViewModel: TransportViewModel

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val database = DatabaseProvider.getDatabase(applicationContext)
        val repository = TransportRepository(database.transportPassDao())

        transportViewModel = TransportViewModel(repository)

        setContent {
            MaterialTheme {
                AppNavigation(
                    viewModel = transportViewModel
                )
            }
        }
    }
}