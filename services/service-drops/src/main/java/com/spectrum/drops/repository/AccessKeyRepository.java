package com.spectrum.drops.repository;

import com.spectrum.drops.model.AccessKey;
import org.springframework.data.mongodb.repository.MongoRepository;

import java.util.List;

public interface AccessKeyRepository extends MongoRepository<AccessKey, String> {
    List<AccessKey> findByUserId(String userId);
}
