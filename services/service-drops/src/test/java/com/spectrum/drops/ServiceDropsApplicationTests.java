package com.spectrum.drops;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;

class ServiceDropsApplicationTests {

    @Test
    void applicationClassCanBeLoaded() {
        assertDoesNotThrow(() -> Class.forName(ServiceDropsApplication.class.getName()));
    }
}
